
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Text;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Hermes.Modules.Services;
using Newtonsoft.Json;
using Color = System.Drawing.Color;
using ImageFormat = System.Drawing.Imaging.ImageFormat;

namespace Hermes.Modules.General
{

    [DiscordCommandClass("General", "Command with the General stuff")]
    class GraphChartOrSmth : CommandModuleBase
    {
        class MessageCache
        {
            public List<Tuple<ulong, ulong>> listOfUsers { get; set; }
            public ulong lastMessageID { get; set; }
        }
        public static List<Color> ColorStructToList()
        {
            return typeof(Color).GetProperties(BindingFlags.Static | BindingFlags.DeclaredOnly | BindingFlags.Public)
                .Select(c => (Color)c.GetValue(null, null))
                .ToList();
        }
        private Brush[] SliceBrushes
        {
            get
            {
                var list = ColorStructToList();
                list.RemoveAll(k =>
                {
                    List<Color> badcolors = new()
                    {
                        Color.White,
                        Color.AliceBlue,
                        Color.Azure,
                        Color.FloralWhite,
                        Color.SeaShell,
                        Color.AntiqueWhite,
                        Color.Bisque,
                        Color.BlanchedAlmond,
                        Color.Cornsilk,
                        Color.GhostWhite,
                        Color.Ivory,
                        Color.Lavender,
                        Color.LavenderBlush,
                        Color.WhiteSmoke,
                        Color.NavajoWhite,
                        Color.Beige,
                        Color.LightGoldenrodYellow,
                        Color.LightYellow
                    };
                    return badcolors.Any(m => m == k);
                });
                return list.OrderBy(_ => Guid.NewGuid()).Select(clr => new SolidBrush(clr)).Cast<Brush>().ToArray();
            }
        }
        // Pens used to outline pie slices.
        private Pen[] SlicePens = { Pens.Black };

        [DiscordCommand("chatchart", commandHelp = "chatchart #channel",
            description =
                "Shows the chat chart for a channel, and no I wont make a cmd that does this for whole server 😫",
            example = "chatchart #weirdshitchannel")]
        public async Task GraphChartOrSmthCommand(params string[] args)
        {
            var messageChannelWhichMightActuallyChangeLMAO = Context.Channel;
            SocketTextChannel channel;
            if (args.Length == 0)
            {
                channel = Context.Channel as SocketTextChannel;
            }
            else
            {
                channel = GetChannel(args[0]) as SocketTextChannel;
            }

            if (channel == null)
            {
                await ReplyAsync("", false, new EmbedBuilder
                {
                    Title = "Invalid channel!",
                    Description = "Well, the channel was invalid. 🤷‍",
                    Color = Discord.Color.Red
                }.WithCurrentTimestamp());
                return;
            }

            List<Tuple<IUser, ulong>> guildUsers = new();
            var dateTimeOffset = DateTimeOffset.MinValue;
            var msg = await ReplyAsync("Querying messages...");
            var s = $"{Context.Guild.Id}_{channel.Id}.rlxc";
            var fe = File.Exists(s);
            var aen = new List<IReadOnlyCollection<IMessage>>().ToAsyncEnumerable();
            if (!fe)
            {
                aen = channel.GetMessagesAsync(int.MaxValue);
            }
            else
            {
                var rat = JsonConvert.DeserializeObject<MessageCache>(await File.ReadAllTextAsync(s));
                guildUsers = rat.listOfUsers
                    .Select(k => new Tuple<IUser, ulong>(Program.Client.GetUser(k.Item1), k.Item2)).ToList();
                guildUsers.RemoveAll(f => f.Item1 == null);
                aen = channel.GetMessagesAsync(rat.lastMessageID, Direction.After, int.MaxValue);
            }

            await foreach (var k in aen)
            {
                for (var i = 0; i < k.Count; i++)
                {
                    var message = k.ElementAt(i);
                    if (i == 0) Console.WriteLine($"queried {i}");
                    if (message.Author.IsBot) continue;

                    var numAlrMsgs = guildUsers.Any(h => h.Item1.Id == message.Author.Id)
                        ? guildUsers.First(h => h.Item1.Id == message.Author.Id).Item2
                        : 0;
                    guildUsers.RemoveAll(h => h.Item1.Id == message.Author.Id);
                    guildUsers.Add(new Tuple<IUser, ulong>(message.Author, numAlrMsgs + 1));


                }
            }

            ;
            // var messa = await channel.SendMessageAsync("Hermes completed querying. This is a landmark message.\n**DO NOT DELETE THIS MESSAGE**");
            var messa = (await channel.GetMessagesAsync(1).FlattenAsync()).First();
            await messa.AddReactionAsync(new Emoji("🏁"));
            var mes = new MessageCache
            {
                listOfUsers = guildUsers.Select(k => new Tuple<ulong, ulong>(k.Item1.Id, k.Item2)).ToList(),
                lastMessageID = messa.Id,
            };
            var des = JsonConvert.SerializeObject(mes);
            await File.WriteAllTextAsync(s, des);
            var count = guildUsers.Sum(dj => float.Parse(dj.Item2.ToString()));
            var majorUsers = guildUsers.OrderBy(m => m.Item2) as IEnumerable<Tuple<IUser, ulong>>;
            var selected = majorUsers.Where(k => (k.Item2 / count) > 0.02).ToList();
            var nextsum = selected.Sum(dj => float.Parse(dj.Item2.ToString()));
            var rest = count - nextsum;
            var toStr = selected.Select(m => new Tuple<string, ulong>(m.Item1.ToString(), m.Item2)).ToList();
            
            var bitmap = new Bitmap(2000, 1000);
            var gr = Graphics.FromImage(bitmap);
            var sb = SliceBrushes;
            Font font;
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) font = new Font(FontFamily.Families.First(k => k.Name == "Segoe UI Semibold"), 30);
            else
            {
                var coll = new PrivateFontCollection();
                coll.AddFontFile("/home/Hermes/seguisb.ttf");
                font = coll.Families.Length == 0 ? new Font(SystemFonts.DefaultFont, FontStyle.Regular) : new Font(coll.Families[0],30);

            }

            toStr.Reverse();
            if (rest > 0)
                toStr.Add(new Tuple<string, ulong>("Others", ulong.Parse(rest.ToString(CultureInfo.InvariantCulture))));
            DrawPieChart(gr, new Rectangle(200, 200, 600, 600), -90, sb, SlicePens,toStr , Brushes.White, font, count, channel);

            bitmap.Save("chart.jpeg", ImageFormat.Jpeg);
            var max = selected.Max(k => k.Item2);
            var max2 = selected.Max(m => m.Item2 == max ? 0 : m.Item2);
            var max3 = selected.Max(m => (m.Item2 == max || m.Item2 == max2) ? 0 : m.Item2);
            var sf = selected.First(m => m.Item2 == max);
            var ss = selected.FirstOrDefault(l => l.Item2 == max2);
            var st = selected.FirstOrDefault(l => l.Item2 == max3);
            await messageChannelWhichMightActuallyChangeLMAO.SendFileAsync("chart.jpeg", $"The top 3 in this channel are ~ \n🥇 {sf.Item1} - {sf.Item2}\n" +
                                                              (ss == null ? "" : $"🥈 {ss.Item1} - {ss.Item2}\n") +
                                                              (st == null ? "" : $"🥉 {st.Item1} - {st.Item2}"));
        }
        private static void DrawPieChart(Graphics gr,
            Rectangle rect, float initial_angle, IReadOnlyList<Brush> brushes, IReadOnlyList<Pen> pens,
            IReadOnlyList<Tuple<string, ulong>> lis, Brush label_brush, Font label_font, float total, IChannel chnl)
        {
            var start_angle = initial_angle;
            var values = lis.Select(mk => mk.Item2).ToArray();
            gr.DrawString($"Stats in {chnl.Name}", label_font, label_brush, new PointF(280,50));
            for (var i = 0; i < values.Length; i++)
            {
                var sweep_angle = values[i] * 360f / total;

                // Fill and outline the pie slice.
                gr.FillPie(brushes[i % brushes.Count],
                    rect, start_angle, sweep_angle);
                gr.DrawPie(pens[i % pens.Count],
                    rect, start_angle, sweep_angle);
                gr.FillRectangle(brushes[i % brushes.Count], new Rectangle(1000,100 + 60*i,120, label_font.Height));
                gr.DrawString(lis[i].Item1 + $" - {Convert.ToInt32(100 * (lis[i].Item2 / total))}%", label_font, label_brush, new PointF(1140, 100 + 60*i));
                start_angle += sweep_angle;
            }
            /*
            // Label the slices.
            // We label the slices after drawing them all so one
            // slice doesn't cover the label on another very thin slice.
            using (StringFormat string_format = new StringFormat())
            {
                // Center text.
                string_format.Alignment = StringAlignment.Center;
                string_format.LineAlignment = StringAlignment.Center;

                // Find the center of the rectangle.
                float cx = (rect.Left + rect.Right) / 2f;
                float cy = (rect.Top + rect.Bottom) / 2f;

                // Place the label about 2/3 of the way out to the edge.
                float radius = (rect.Width + rect.Height) / 2f * 0.33f;

                start_angle = initial_angle;
                for (int i = 0; i < values.Length; i++)
                {
                    float sweep_angle = values[i] * 360f / total;

                    // Label the slice.
                    double label_angle =
                        Math.PI * (start_angle + sweep_angle / 2f) / 180f;
                    float x = cx + (float)(radius * Math.Cos(label_angle));
                    float y = cy + (float)(radius * Math.Sin(label_angle));
                    gr.DrawString(lis[i].Item1 + $" - {Convert.ToInt32(100 * (lis[i].Item2 / total))}",
                        label_font, label_brush, x, y, string_format);

                    start_angle += sweep_angle;
                }*/
            }
        }
    }

