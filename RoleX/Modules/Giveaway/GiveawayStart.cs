
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using MoreLinq;
using RoleX.Modules.Services;

namespace RoleX.Modules.Giveaway
{
    [DiscordCommandClass("Giveaways", "The Module with Giveaway Commands")]
    public class Giveaway : CommandModuleBase
    {

        private EmbedBuilder MyEmbedBuilder = new();
        private readonly Emoji dice = new("🎉");
        private readonly Emoji trophy = new("🏆");
        private readonly Random rand = new();


        //Initates command, summary, and assigns permissions 
        [DiscordCommand("gstart", description = "Starts a giveaway, kinda obvious ngl. IN BETA, NOT FOR USE!", commandHelp = "gstart <time> <num-winners> <requirement> <prize>", example = "gstart 1m 1w none Weird prize")]
        [RequiredUserPermissions(GuildPermission.Administrator)]
        [RequiredBotPermission(GuildPermission.AddReactions)]
        // string time, string prize, string type
        public async Task gAway(params string[] args)
        {
            switch (args.Length)
            {
                case 0 or 1 or 2 or 3:
                    await ReplyAsync("", false, new EmbedBuilder
                    {
                        Title = "Invalid Arguments!",
                        Description =
                            $"Command Syntax: `{await SqliteClass.PrefixGetter(Context.Guild.Id)}gstart <time> <num-winners> <requirement> <prize>`",
                        Color = Color.Red
                    });
                    return;
            }

            var hostedBy = Context.User;
            var time = args[0];
            var numWinners = args[1].Replace("w", "");
            if (!uint.TryParse(numWinners, out uint _))
            {
                await ReplyAsync("", false, new EmbedBuilder
                {
                    Title = "Invalid Arguments!",
                    Description =
                        $"Couldn't parse {numWinners} as int",
                    Color = Color.Red
                });
                return;
            }
            var requirement = args[2];
            var prize = string.Join("", string.Join(" ", args.Skip(3)).Take(100));
            Console.WriteLine("Starting giveaway");
            var t = int.Parse(time.Remove(time.Length - 1));
            var subsetSize = int.Parse(numWinners);
            var commonFooter = new EmbedFooterBuilder
            {
                Text = $"Winners: {subsetSize}"
            };

            //Starts the Embeded Message
            MyEmbedBuilder.Title = prize;
            MyEmbedBuilder.Color = Blurple;

            //Changes message if time is seconds or minutes
            if (time.Contains('h'))
            {

                MyEmbedBuilder.Description = $"\nReact with {dice} to win!\n" + "Time remaining: " +
                                                 t + " hours";
                t *= 3600;

            }

            if (time.Contains('m'))
            {
                MyEmbedBuilder.Description = $"\nReact with {dice} to win!\n" + "Time remaining: " +
                                                 t + " minutes";

                t *= 60;

            }
            else
            {
                MyEmbedBuilder.Description = $"\nReact with {dice} to win!\n" + "Time remaining: " +
                                                 t + " seconds";

            }

            MyEmbedBuilder.Description += $"\nHosted by: {hostedBy.Mention}";
            MyEmbedBuilder.Footer = commonFooter;
            MyEmbedBuilder = MyEmbedBuilder.WithCurrentTimestamp();
            var commonTS = MyEmbedBuilder.Timestamp;
            //Sends message
            var message = await Context.Channel.SendMessageAsync("🎉 **GIVEAWAY** 🎉", false, MyEmbedBuilder.Build());

            //Reacts to message
            await message.AddReactionAsync(dice);
            Stopwatch sw = new();
            //Begins countdown and edits embeded field every hour, minute, or second
            while (t > 0)
            {
                await Task.Delay(200);
                t--;
                var newMessage = await message.Channel.GetMessageAsync(message.Id) as IUserMessage;
                var embed2 = new EmbedBuilder
                {
                    Title = prize,
                    Color = Blurple
                };
                embed2.Timestamp = commonTS;
                switch (t)
                {
                    case >= 3600:
                    {
                        var t3 = t;
                        t3 /= 3600;
                        var time_minutes = (t / 60) % 60;

                        embed2.Description = $"\nReact with {dice} to win!\n" + "Time remaining: " +
                                               t3 + " hours " + time_minutes + " minutes";
                        break;
                    }
                    case >= 60 and < 3600:
                    {
                        var t2 = t;
                        t2 /= 60;
                        var time_seconds = t % 60;
                        embed2.Description = $"\nReact with {dice} to win!\n" + "Time remaining: " +
                                               t2 + " minutes " + time_seconds + " seconds";
                        break;
                    }
                    case < 60:
                        embed2.Description = $"\nReact with {dice} to win!\n" + "Time remaining: " + 
                                               t + " seconds";
                        break;
                }

                embed2.Description += $"\nHosted by: {hostedBy.Mention}";
                embed2.Footer = commonFooter;
                await newMessage.ModifyAsync(m => m.Embed = embed2.Build());

            }


            //Adds users to list and randomly selects winner
            var temp = await message.GetReactionUsersAsync(dice, int.MaxValue).FlattenAsync();

            var users = temp.ToList();
            users.RemoveAll(k => k.IsBot);
            if (users.Any())
            {
                var winners = users.Count > subsetSize ? users.RandomSubset(subsetSize).Select(k => k.ToString()).ToList() : users.Select(k => k.ToString());
                var winnerStr = winners.Count() > 1 ? string.Join(",", winners.Skip(1)) + $" and {winners.First()}" : winners.First();
                var message3 = await message.Channel.GetMessageAsync(message.Id) as IUserMessage;
                var embed3 = new EmbedBuilder();
                embed3.Title = prize;
                embed3.Description = $"Winner(s): {winnerStr}\nHosted by: {hostedBy.Mention}";
                await message3.ModifyAsync(m =>
                {
                    m.Content = "🎉 **GIVEAWAY ENDED** 🎉";
                    m.Embed = embed3.Build();
                });
                embed3.Timestamp = commonTS;
                embed3.Footer = commonFooter;
                await message3.Channel.SendMessageAsync("", false, new EmbedBuilder().WithTitle("Giveaway Ended").WithDescription($"```{winnerStr} has won the giveaway for {prize}```\n[Jump]({message3.GetJumpUrl()})").WithColor(Blurple).WithCurrentTimestamp().Build());
            }
            else
            {
                var message4 = await message.Channel.GetMessageAsync(message.Id) as IUserMessage;
                var embed4 = new EmbedBuilder();
                embed4.Title = prize;
                embed4.Description = $"Winner: none\nHosted by: {hostedBy.Mention}";
                await message4.ModifyAsync(m =>
                {
                    m.Content = "🎉 **GIVEAWAY ENDED** 🎉";
                    m.Embed = embed4.Build();
                });
                embed4.Timestamp = commonTS;
                embed4.Footer = commonFooter;
                await message4.Channel.SendMessageAsync("", false, new EmbedBuilder().WithTitle("Giveaway Ended").WithDescription($"```There were no valid entries for the {prize} giveaway```\n[Jump]({message4.GetJumpUrl()})").WithColor(Blurple).WithCurrentTimestamp().Build());
            }
        }
    }

}