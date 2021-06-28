using RoleX.Modules.Services;
using System.Threading.Tasks;
using Discord;
using System.Linq;
namespace RoleX.Modules.Giveaway_Module
  
{
    [DiscordCommandClass("Giveaways", "The module for giveaways!!")]
    public class GEnd : CommandModuleBase
    {
        [RequiredUserPermissions(GuildPermission.ManageGuild)]
        [Alt("giveawayend")]
        [Alt("gawend")]
        [DiscordCommand("gend", commandHelp = "gstart <msg-id/link>", description = "Ends a giveaway", example = "gend 490583492850908932")]
        public async Task gEndCommand(params string[] args)
        {
            if (args.Length == 0)
            { await ReplyAsync("You gotta tell me what to remove and where :/"); return; }
            var link = args[0];
            ulong chnlid;
            if (ulong.TryParse(link, out var msgid))
            {
                chnlid = Context.Channel.Id;
            }
            else
            {
                var dry = link.Replace(@"https://discord.com/channels/", "").Split('/');
                chnlid = ulong.Parse(dry[1]);
                msgid = ulong.Parse(dry[2]);
            }
            var channel = Context.Guild.GetTextChannel(chnlid);
            if (channel == null)
            {
                await ReplyAsync("", false, new EmbedBuilder
                {
                    Title = "Which message?",
                    Description = $"Couldn't parse `{args[0]}` as a Discord Message link\nHint: We cannot find the channel. This might be due to Permissions, or that Channel (and Message) is from another server.",
                    Color = Color.Red
                }.WithCurrentTimestamp());
                return;
            }
            var message = await channel.GetMessageAsync(msgid);
            if (message == null)
            {
                await ReplyAsync("", false, new EmbedBuilder
                {
                    Title = "Which message?",
                    Description = $"Couldn't parse `{args[0]}` as a Discord Message link\nHint: We cannot find the message. This might be due to deletion or permissions.",
                    Color = Color.Red
                }.WithCurrentTimestamp());
                return;
            }
            var gaw = new Giveaway_Module.gstart.Giveaway()
            {
                GuildID = Context.Guild.Id,
                ChannelID = chnlid,
                MessageID = msgid
            };
            var k = (await SqliteClass.GetGiveaways($"SELECT * from giveaways WHERE GuildId = {Context.Guild.Id} AND ChannelID = {chnlid} AND MessageID = {msgid};")).FirstOrDefault();
            if (k == null)
            {
                await ReplyAsync("No such giveaway exists :/");
                return;
            }
            await SqliteClass.GiveawayRemover(gaw);
            if (message == null) return;
            if (message is not IUserMessage mymsg) return;
            await mymsg.ModifyAsync(msgprop =>
            {
                if (!mymsg.Content.Contains("ENDED")) msgprop.Content = mymsg.Content.Replace("GIVEAWAY", "GIVEAWAY ENDED");
                var existingEmbed = mymsg.Embeds.First();
                msgprop.Embed = new EmbedBuilder()
                {
                    Title = existingEmbed.Title,
                    Description = existingEmbed.Description.Split('\n')[0] + string.Join('\n', existingEmbed.Description.Split('\n').Skip(2))
                }.WithCurrentTimestamp().Build();
            });
            var allWhoReacted = await mymsg.GetReactionUsersAsync(new Emoji("🎉"), int.MaxValue).FlattenAsync();
            if (Context.Guild == null) return;
            var allWhoReactedButDidntLeave = allWhoReacted.Where(user => Context.Guild.GetUser(user.Id) is not null && !user.IsBot).Select(o => Context.Guild.GetUser(o.Id));
            var andAllWhoMetRequirements = allWhoReactedButDidntLeave.Where(kden =>
                k.RoleReqs.TrueForAll(role => kden.Roles.Any(m => m.Id == role.Id)));
            System.Random rnd = new();
            string mentions = "";
            if (k.Winners == 0 || andAllWhoMetRequirements.Count() == 0) { mentions = "nobody"; }
            else
            {
                for (int i = 0; i < k.Winners; i++)
                {
                    var usr = andAllWhoMetRequirements.ElementAt(rnd.Next(0, andAllWhoMetRequirements.Count() - 1));
                    await usr.SendMessageAsync("", false, new EmbedBuilder
                    {
                        Title = $"You have won **{k.Title}** in {Context.Guild.Name}!",
                        Description = $"DM <@{k.StarterID}> for your prize",
                        Color = CommandModuleBase.Blurple,
                        Url = $"https://discord.com/channels/{k.GuildID}/{k.ChannelID}/{k.MessageID}"
                    }.WithCurrentTimestamp().Build());
                    mentions += mentions == "" ? usr.Mention : $", {usr.Mention}";

                }
            }
            await mymsg.Channel.SendMessageAsync($"Congratulations {mentions}. You have won {k.Title}");
            var strt = Context.Client.GetUser(k.StarterID);
            if (strt == null) return;
            await strt.SendMessageAsync("", false, new EmbedBuilder
            {
                Title = $"Your giveaway ended.",
                Description = $"Your winner(s) are {mentions}",
                Color = Blurple,
                Url = $"https://discord.com/channels/{k.GuildID}/{k.ChannelID}/{k.MessageID}"
            }.WithCurrentTimestamp().Build());
        
        }
    }
}