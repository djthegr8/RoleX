using Discord;
using RoleX.Modules.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection.Metadata.Ecma335;
using System.Threading.Tasks;
using Discord.WebSocket;

namespace RoleX.Modules.Giveaway_Module
{
    [DiscordCommandClass("Giveaways", "The module for giveaways!!")]
    public class gstart : CommandModuleBase
    {
        [Alt("giveawaystart")]
        [Alt("gawstart")]
        [DiscordCommand("gstart", commandHelp = "gstart <time> <winners> <role> <title>", description = "**Starts a giveaway**.\r\nThe winners argument can be provided with or without a 'w' suffixed to it.\r\n\r\nRoles can be added as requirements for the giveaway. To have multiple, seperate them using '|'. In the same parameter, add nn as a role to ensure that only people without nitro can participate in the giveaway.\r\nIf the role name has spaces, use its ID.\r\nPass None to specify no role requirement", example = "gstart 1m 2w none 10k dmc`\n`gstart 10m30s RoleID|Role2ID Nitro Classic!")]
        public async Task gstartCommand(params string[] args)
        {
            if (args.Length < 4)
            {
                await ReplyAsync("", false, new EmbedBuilder()
                {
                    Title = "Insufficient Arguments",
                    Description = $"The way to use the command is `{await SqliteClass.PrefixGetter(Context.Guild.Id)}gstart <time> <winners> <role> <title>`",
                    Color = Color.Red
                }.WithCurrentTimestamp());
                return;
            }
            // Okay then, he's done it right :/
            var  noNitro = false;
            var time = args[0];
            var winners = args[1];
            var roles = args[2];
            var title = string.Join(' ', args.Skip(3));
            var isValidTime = time.Last() switch
            {
                'h' or 'H' or 'm' or 'M' or 'd' or 'D' or 's' or 'S' => true,
                _ => false
            } && int.TryParse(string.Join("", args[0].SkipLast(1)), out _);
            if (!isValidTime)
            {
                await ReplyAsync("", false, new EmbedBuilder
                {
                    Title = "The time parameter is invalid",
                    Description = $"Couldn't parse `{args[0]}` as time, see key below\n```s => seconds\nm => minutes\nh => hours\nd => days```",
                    Color = Color.Red
                }.WithCurrentTimestamp());
                return;
            }

            var timezar = int.Parse(string.Join("", args[0].SkipLast(1)));
            var ts = args[0].Last() switch
            {
                'h' or 'H' => new TimeSpan(timezar, 0, 0),
                'm' or 'M' => new TimeSpan(0, timezar, 0),
                's' or 'S' => new TimeSpan(0, 0, timezar),
                'd' or 'D' => new TimeSpan(timezar, 0, 0, 0),
                //Non possible outcome but IDE is boss
                _ => new TimeSpan()
            };
            var isWinnersOk = int.TryParse(winners.Replace("w", ""),out var numWinners);
            if (winners.ToLower() == "none")
            {
                isWinnersOk = true;
                numWinners = 0;
            }
            if (!isWinnersOk)
            {
                await ReplyAsync("", false, new EmbedBuilder
                {
                    Title = "The winners parameter is invalid",
                    Description = $"Couldn't parse `{args[1]}` as winners, you need to either put the number, or suffix it with `w`, or put none...",
                    Color = Color.Red
                }.WithCurrentTimestamp());
                return;
            }

            var  weeklyAmari = 0;
            var amariLevelRequirement = 0;
            // By now, the time is valid and winners are fine. Time to check the Roles and RoleIDs
            List<SocketRole> listOfRoles = new();
            if (roles.ToLower() != "none")
            {
                foreach (var role in roles.Split('|'))
                {
                    if (role.ToLower() == "nn")
                    {
                        noNitro = true;
                        continue;
                    }

                    if (role.Contains("wa"))
                    {
                        var rr = role.Replace("wa", "");
                        if (!int.TryParse(rr, out var ir))
                        {
                            await ReplyAsync("", false, new EmbedBuilder
                            {
                                Title = "The Weekly Amari is invalid",
                                Description = $"Couldn't parse `{role}` as weekly amari. Use the format `amari{{AmariLevelReq}}`, for example `amari100`",
                                Color = Color.Red
                            }.WithCurrentTimestamp());
                            return;
                        }

                        weeklyAmari = ir;
                        continue;
                    } else if (role.Contains("amari"))
                    {
                        var xx = role.Replace("amari", "");
                        if (!int.TryParse(xx, out var ix))
                        {
                            await ReplyAsync("", false, new EmbedBuilder
                            {
                                Title = "The Level Amari is invalid",
                                Description = $"Couldn't parse `{role}` as weekly amari. Use the format `wa{{AmariLevelReq}}`, for example `wa100`",
                                Color = Color.Red
                            }.WithCurrentTimestamp());
                            return;
                        }

                        amariLevelRequirement = ix;
                        continue;
                    }
                    var rl = GetRole(role);
                    listOfRoles.Add(rl);
                    if (rl != null) continue;
                    await ReplyAsync("", false, new EmbedBuilder
                    {
                        Title = "The role parameter is invalid",
                        Description = $"Couldn't parse `{role}` as a role!",
                        Color = Color.Red
                    }.WithCurrentTimestamp());
                    return;
                }
            }

            var mes = await Context.Channel.SendMessageAsync("🎉 **GIVEAWAY** 🎉", false, new EmbedBuilder()
            {
                Title = title,
                Description = $"React with 🎉 to enter\nTime left: {((ts.TotalHours >= 1) ? $"**{ts.TotalHours}** hour(s) and **{ts.Minutes}** minutes" : $"**{ts.Minutes}** minutes and **{ts.Seconds}** seconds")}{(!listOfRoles.Any() ? "" : $"\nRequirement: {string.Join('\n', listOfRoles.Select(rl => rl.Mention))}")}{(amariLevelRequirement > 0 ? $"\nAmari Level Requirement: {amariLevelRequirement}" : "")}{(weeklyAmari > 0 ? $"\nMinimum Weekly Amari: {weeklyAmari}" : "")}\nHosted by: {Context.User}",
                Color = listOfRoles.Count == 0 ? Blurple : listOfRoles.First().Color,
                ThumbnailUrl = Context.User.GetAvatarUrl()
            }.WithFooter($"Winners: {numWinners} | Ends at :").WithTimestamp(DateTime.UtcNow.Add(ts)).Build());
            await mes.AddReactionAsync(new Emoji("🎉"));
            var giveaway = new Giveaway()
            {
                Title = title,
                Winners = numWinners,
                GuildID = Context.Guild.Id,
                ChannelID = Context.Channel.Id,
                MessageID = mes.Id,
                StarterID = Context.User.Id,
                EndingTime = DateTime.UtcNow.Add(ts),
                RoleReqs = listOfRoles,
                NoNitroReq = noNitro,
                WeeklyAmariRequirement = weeklyAmari,
                AmariLevelRequirement = amariLevelRequirement
            };
            await SqliteClass.GiveawayAdder(giveaway);
            
        }

        public class Giveaway
        {
            /// <summary>
            /// The ID of the <see cref="SocketGuild"/>
            /// </summary>
            public ulong GuildID { get; set; }
            /// <summary>
            /// The ID of the <see cref="SocketTextChannel"/>
            /// </summary>
            public ulong ChannelID { get; set; }
            /// <summary>
            /// Number of winners
            /// </summary>
            public int Winners { get; set; }
            /// <summary>
            /// The ID of the <see cref="SocketUserMessage"/>
            /// </summary>
            public ulong MessageID { get; set; }
            /// <summary>
            /// Gaw starters ID
            /// </summary>            
            public ulong StarterID { get; set; }
            /// <summary>
            /// List of roles required to have for a user to have to enter gaw.
            /// <c>ONLY FOR SETTING.</c>
            /// </summary>
            public List<SocketRole> RoleReqs { get; set; }
            /// <summary>
            /// Level req for Amari
            /// </summary>
            public int AmariLevelRequirement { get; set; } = 0;
            /// <summary>
            /// Weekly req for Amari
            /// </summary>
            public int WeeklyAmariRequirement { get; set; } = 0;
            /// <summary>
            /// Title of the gaw
            /// </summary>
            public string Title { get; set; }
            /// <summary>
            /// When should the gaw end
            /// </summary>
            public DateTime EndingTime
            {
                get { return endingTime.ToUniversalTime(); }
                set => endingTime = value;
            }
            private DateTime endingTime;
            /// <summary>
            /// Whether u cannot have nitro when you join
            /// </summary>
            public bool NoNitroReq { get; set; } = false;
            
            public string RoleRequirementString
            {
                get { return string.Join('|', RoleReqs.Select(k => k.Id)); }
                set
                {
                    //RoleReqs = Program.Client.GetGuild(GuildID).Roles
                    if (value == "") { RoleReqs = new(); return; }
                    var roleIDS = value.Split('|');
                    var gld = Program.Client.GetGuild(GuildID);
                    if (gld == null) return;
                    RoleReqs = roleIDS.Select(k => gld.GetRole(ulong.Parse(k))).ToList();
                }
            }
            public Giveaway()
            {
                
            }
        }
    }
}
