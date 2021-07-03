using Discord;
using Hermes.Modules.Services;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Hermes.Modules.Channel_Permission
{
    [DiscordCommandClass("Channel Editor", "Edit Channel-wise perms of a Role using these commands!")]
    public class Categorydelete : CommandModuleBase
    {
        [RequiredUserPermissions(GuildPermission.ManageChannels)]
        [Alt("catdel")]
        [DiscordCommand("categorydelete", commandHelp = "categorydelete <category-name>", description = "Deletes given category and all its channels", example = "categorydelete Useless")]
        public async Task CatDel(params string[] aa)
        {
            if (aa.Length == 0) aa = new[] { Context.Channel.Id.ToString() };
            var alf = GetCategory(aa[0]);
            if (alf == null)
            {
                await ReplyAsync("", false, new EmbedBuilder
                {
                    Title = "Invalid category",
                    Description = $"`{aa}` could not be parsed as category!",
                    Color = Color.Red
                }.WithCurrentTimestamp());
                return;
            }
            Emote cros = Emote.Parse("<a:cros:859033035545378826>");
            Emote tickk = Emote.Parse("<a:tick:859032462410907649>");
            var gc = Guid.NewGuid();
            var cb = new ComponentBuilder().
                WithButton("", $"{gc}Tick", ButtonStyle.Secondary, tickk).
                WithButton("", $"{gc}Cros", ButtonStyle.Secondary, cros);
            var ram = await Context.Channel.SendMessageAsync($"Are you sure you want to delete `{alf.Name}`?\nThis is a potentially destructive action.", component: cb.Build());
            CancellationTokenSource cancelSource = new CancellationTokenSource();
            cancelSource.CancelAfter(15000);
            var Interaction = await InteractionHandler.NextButtonAsync(k => k.Data.CustomId.Contains(gc.ToString()) && k.User.Id == Context.User.Id, cancelSource.Token);
            if (Interaction == null)
            {
                await Context.Channel.SendMessageAsync("No response received!");
                return;
            }
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

            foreach (var ch in alf.Channels)
            {
                await ch.DeleteAsync();
            }
            await alf.DeleteAsync();
            try
            {
                await ReplyAsync("", false, new EmbedBuilder
                {
                    Title = "Action successful!",
                    Description = $"<#{alf.Id}> was deleted along with all its channels",
                    Color = Blurple
                }.WithCurrentTimestamp());
            }
            catch
            {
                Console.WriteLine("Cat del'd");
            }

        }


    }
}