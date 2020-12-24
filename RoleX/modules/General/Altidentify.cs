using System;
using static RoleX.Modules.SqliteClass;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Public_Bot;
using System.Threading.Tasks;
using Discord.WebSocket;
using Discord;
using GuildPermissions = Public_Bot.GuildPermissions;
namespace RoleX.Modules
{
    [DiscordCommandClass("General", "General commands for all!")]
    public class Altidentify : CommandModuleBase
    {
        // [DiscordCommand("altidentify", commandHelp = "altidentify <number-of-alts>", description = "Finds the x users newest to Discord and most probable alts")]
        //public async Task Yu(params string[] argz)
        //{
        //    var test = 10;
        //    if (int.TryParse(argz.FirstOrDefault(), out int retest))
        //    {
        //        test = retest;
        //    }
        //    if (test > Context.Guild.MemberCount)
        //    {
        //        await Context.Channel.SendMessageAsync(embed:new EmbedBuilder
        //        {
        //            Title="This guild does not have the specified amount of users",
        //            Description=$"You asked for {test} youngest users, while your server has only {Context.Guild.MemberCount}",
        //            Color = Blurple
        //        }.WithCurrentTimestamp()
        //        );
        //        return;
        //    }
        //    await ReplyAsync("wait a sec, im getting all the users..");
        //    await Context.Guild.DownloadUsersAsync();
        //    await ReplyAsync("Done i guess");
        //    var yus = Context.Guild.Users;
        //    string cty = "```";
        //    var tenYoungestUsers = yus.ToList();
        //    tenYoungestUsers.RemoveAll(x => x.IsBot);
        //    try
        //    {
        //        tenYoungestUsers.Sort((prev, next) => DateTimeOffset.Compare(prev.CreatedAt, next.CreatedAt));
        //    }
        //    catch
        //    {
        //        tenYoungestUsers.Sort((prev, next) => 0);
        //    }
        //    tenYoungestUsers.Reverse();
        //    var current = tenYoungestUsers.GetRange(0, test);
        //    var pr = current.Max(rx => (rx.Username + "#" + rx.Discriminator + $"({rx.Id})").Length) + '\t';
        //    current.ForEach(x => cty += ((x.Username + "#" + x.Discriminator + $"({x.Id})").PadRight(pr) + $"{x.CreatedAt.Month}/{x.CreatedAt.Day}/{x.CreatedAt.Year}" + '\n'));
        //    cty += "```";
        //    var mmbed = new EmbedBuilder
        //    {
        //        Title = $"Youngest Users in {Context.Guild.Name}",
        //        Description = cty,
        //        Color = Blurple
        //    }.WithCurrentTimestamp();
        //    await Context.Channel.SendMessageAsync("", false, mmbed);
        //}

        [DiscordCommand("invite",description ="Invite RoleX to your server!!", commandHelp ="invite")]
        public async Task Invite(params string[] _)
        {
            await ReplyAsync("", false, new EmbedBuilder
            {
                Title = "Invite RoleX to your server!",
                Description = "[**Invite using recommended permission link**](https://discord.com/oauth2/authorize?client_id=744766526225252435&scope=bot&permissions=805334064)\n[**Invite using Admin link for easy setup**](https://discord.com/oauth2/authorize?client_id=744766526225252435&scope=bot&permissions=8)",
                Footer = new EmbedFooterBuilder()
                {
                    Text="Thank you for choosing RoleX"
                },

                Color = Blurple
            }.WithCurrentTimestamp());
        }
    }
}