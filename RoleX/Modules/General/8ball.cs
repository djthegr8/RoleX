using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using RoleX.Modules.Services;

namespace RoleX.Modules.General
{
    [DiscordCommandClass("General", "General commands for all!")]
    public class _8ball : CommandModuleBase
    {
        private static string[] Answers = {
            "It is certain.",
            "It is decidedly so.",
            "Without a doubt.",
            "Yes – definitely.",
            "You may rely on it.",
            "As I see it, yes.",
            "Most likely.",
            "Outlook good.",
            "Yes.",
            "Signs point to yes.",
            "Reply hazy, try again.",
            "Ask again later.",
            "Better not tell you now.",
            "Cannot predict now.",
            "Concentrate and ask again.",
            "Don't count on it.",
            "My reply is no.",
            "My sources say no.",
            "Outlook not so good.",
            "Very doubtful.",
        };
        [DiscordCommand("8ball", commandHelp = "8ball <question>", description = "Asks the 8ball a question", example = "8ball Will I ever succeed?`\n> Absolutely not.")]
        public async Task _8ballCmd(params string[] args)
        {
            if (args.Length == 0)
            {
                // Error

                await Context.Channel.SendMessageAsync("", false, new EmbedBuilder
                {
                    Title = "Sorry what?",
                    Description = "The magic 8 ball knows everything except what you are thinking <:thinkcat:780422740091338772>",
                    Color = Color.Red
                }.WithCurrentTimestamp().Build());
                return;
            }

            string question = string.Join(" ", Context.Message.Content.Split(' ').Skip(1));

            int seed = 0;

            foreach (char c in question)
                seed += c;

            var indx = new Random(seed).Next(Answers.Length);

            var av = Context.User.GetAvatarUrl() ?? Context.User.GetDefaultAvatarUrl();

            await Context.Channel.SendMessageAsync("", false, new EmbedBuilder
            {
                Author = new EmbedAuthorBuilder
                {
                    IconUrl = av,
                    Name = Context.User.Username,
                },
                Description = $"```{question}```\n> **{Answers[indx]}**",
                Color = Blurple,
            }.WithCurrentTimestamp().Build());
        }
    }
    }
