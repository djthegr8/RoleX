﻿using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Discord;
using Discord.Webhook;
using Hermes.Modules.Services;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;

namespace Hermes.Modules.Developer
{
    [DiscordCommandClass("Developer", "Dev commands that you can't use 🤣!")]
    public class Execute : CommandModuleBase
    {
        [Alt("runcode")]
        [DiscordCommand("eval", commandHelp = "", description = "")]
        public async Task GPe(params string[] args)
        {
            if (devids.Any(x => x == Context.User.Id))
            {
                var joined = string.Join(" ",
                    Context.Message.Content.Replace("```cs", "").Replace("```", "").Split(' ').Skip(1));
                var create = CSharpScript.Create(joined,
                    ScriptOptions.Default.WithImports("System", "System.Threading.Tasks", "System.Linq")
                        .WithReferences(Assembly.GetAssembly(typeof(EmbedBuilder)),
                            Assembly.GetAssembly(typeof(DiscordWebhookClient)), Assembly.GetExecutingAssembly())
                        .WithImports("Discord", "Discord.WebSocket"),
                    typeof(CustomCommandGlobals));
                try
                {
                    var state = await create.RunAsync(new CustomCommandGlobals(Context));
                    if (state.ReturnValue == null)
                        await Context.Message.AddReactionAsync(Emote.Parse("<a:tick:859032462410907649>"));
                }
                catch (CompilationErrorException cee)
                {
                    await ReplyAsync("", false, new EmbedBuilder
                    {
                        Title = "'Twas an error",
                        Description = cee.Message,
                        Color = Color.Red
                    }.WithCurrentTimestamp());
                }
                catch
                {
                    // um irdc
                }
            }
        }
    }
}