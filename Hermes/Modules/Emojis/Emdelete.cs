using System;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Hermes.Modules.Services;

namespace Hermes.Modules.Emojis
{
    [DiscordCommandClass("Emote Editor", "For complete management of server emotes!")]
    public class Emdelete : CommandModuleBase
    {
        [RequiredUserPermissions(GuildPermission.ManageEmojisAndStickers)]
        [Alt("emdel")]
        [DiscordCommand("emdelete", description = "Deletes given emoji.", example = "emdelete kekw",
            commandHelp = "emrename emoji_name")]
        public async Task EMDEL(params string[] args)
        {
            if (args.Length == 0)
            {
                await ReplyAsync("", false, new EmbedBuilder
                {
                    Title = "Emoji not provided",
                    Description =
                        $"Command Syntax: `{await SqliteClass.PrefixGetter(Context.Guild.Id)}emdelete emote_name`",
                    Color = Color.Red
                }.WithCurrentTimestamp());
                return;
            }

            if (await GetEmote(args[0]) == null)
            {
                await ReplyAsync("", false, new EmbedBuilder
                {
                    Title = "Emoji not provided?",
                    Description = $"Couldn't parse `{args[0]}` as an emote",
                    Color = Color.Red
                }.WithCurrentTimestamp());
                return;
            }

            var i = await GetEmote(args[0]);
            var cros = Emote.Parse("<a:cros:859033035545378826>");
            var tickk = Emote.Parse("<a:tick:859032462410907649>");
            var gc = Guid.NewGuid();
            var cb = new ComponentBuilder().WithButton("why_does_this", $"{gc}Tick", ButtonStyle.Secondary, tickk)
                .WithButton("not_work", $"{gc}Cros", ButtonStyle.Secondary, cros);
            await Context.Channel.SendMessageAsync(
                $"Are you sure you want to delete {i}?\nThis is a potentially destructive action.",
                component: cb.Build());
            var cancelSource = new CancellationTokenSource();
            cancelSource.CancelAfter(15000);
            var Interaction = await InteractionHandler.NextButtonAsync(
                k => k.Data.CustomId.Contains(gc.ToString()) && k.User.Id == Context.User.Id, cancelSource.Token);
            if (Interaction == null)
            {
                await ReplyAsync("No response received!");
                return;
            }

            await Interaction.AcknowledgeAsync();
            var isTick = Interaction.Data.CustomId.Contains("Tick");
            if (!isTick)
            {
                await Context.Channel.SendMessageAsync("", false, new EmbedBuilder
                {
                    Title = "Alright then...",
                    Color = Blurple,
                    ImageUrl = "https://i.imgur.com/RBC7KUt.png"
                }.WithCurrentTimestamp().Build());
                return;
            }

            var in_ = i.Name;
            await Context.Guild.DeleteEmoteAsync(i);
            await ReplyAsync("", false, new EmbedBuilder
            {
                Title = "Action successful!",
                Description = $"`:{in_}:` was deleted successfully!",
                Color = Blurple
            }.WithCurrentTimestamp());
        }
    }
}
