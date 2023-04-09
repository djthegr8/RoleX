using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Hermes.Modules.Services;
using Hermes.Utilities;
using Urbandic;
namespace Hermes.Modules.General
{
    [DiscordCommandClass("General", "General commands for all!")]
    public class Udict : CommandModuleBase
    {
        [Alt("define")]
        [DiscordCommand("df", commandHelp = "df <query>", description = "Searches urbandictionary to get the most (ir)relevant result",
            example = "No")]
        public async Task DefAsync(params string[] args)
        {
            if (args.Length == 0)
            {
                // Error

                await Context.Channel.SendMessageAsync("", false, new EmbedBuilder
                {
                    Title = "No search term given",
                    Description =
                        "Why are you like this",
                    Color = Color.Red
                }.WithCurrentTimestamp().Build());
                return;
            }

            var question = string.Join(" ", Context.Message.Content.Split(' ').Skip(1));
            var op = UrbanDictionary.Search(question,1);
            if (op == null)
            {
                // Embed saying no results found
                await ReplyAsync("No result :(");
            }
            var paginatedMessage = new PaginatedMessage(PaginatedAppearanceOptions.Default, Context.Channel,
                new PaginatedMessage.MessagePage("Loading..."))
            {
                Title = question,
                Color = Blurple,
                Timestamp = DateTime.Now,
            };
            var embb = new List<EmbedFieldBuilder>();
            // \n\n**Example**:\n{resp.example}\n\n**Author**:\n{resp.author}\n\n**Votes**:\nUpvotes:{resp.thumbs_up}\nDownvotes:{resp.thumbs_down}
            foreach (var resp in op)
            {
                embb.Add(new EmbedFieldBuilder()
                {
                    Name="Definition",
                    Value = resp.definition,
                    IsInline = false
                });
                embb.Add(new EmbedFieldBuilder()
                {
                    Name = "Example",
                    Value = resp.example,
                    IsInline = false
                });
                embb.Add(new EmbedFieldBuilder()
                {
                    Name = "Author",
                    Value = resp.author,
                    IsInline = false
                });
                embb.Add(new EmbedFieldBuilder()
                {
                    Name = "Votes",
                    Value = $"Upvotes: {resp.thumbs_up}\nDownvotes: {resp.thumbs_down}",
                    IsInline = false

                });

            }
            Console.WriteLine(embb.Count);
            paginatedMessage.SetPages("", embb, 4);
            await paginatedMessage.Resend();

        }
    }
}