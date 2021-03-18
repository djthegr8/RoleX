using System.Linq;
using System.Threading.Tasks;
using Discord;
using RoleX.Modules.Services;

namespace RoleX.Modules.Webhooks
{
    [DiscordCommandClass("Webhook Manager", "Helps manage all webhooks!")]
    public class Deletewh : CommandModuleBase
    {
        [RequiredUserPermissions(GuildPermission.ManageWebhooks)]
        [DiscordCommand("deletewh", commandHelp = "deletewh <webhook-name/id>", description = "Deletes the first webhook of given name or ID", example = "deletewh MyWebhook")]
        public async Task DeleteWh(params string[] args)
        {
            if (args.Length == 0)
            {
                await ReplyAsync("", false, new EmbedBuilder
                {
                    Title = "No webhook name/id given",
                    Description = $"There were no arguments given",
                    Color = Color.Red
                }.WithCurrentTimestamp());
                return;
            }
            var allGWH = await Context.Guild.GetWebhooksAsync();
            var iGTSW = allGWH.ToList().FindAll(x => x.Name.ToLower().Contains(args[0].ToLower()));
            if (iGTSW == null || iGTSW?.Count == 0)
            {
                await ReplyAsync("", false, new EmbedBuilder
                {
                    Title = "No webhook found with that name!",
                    Description = $"There were no webhooks found with the word `{args[0]}`",
                    Color = Color.Red
                }.WithCurrentTimestamp());
                return;
            }

            if (iGTSW.Count == 1)
            {
                var reqwh = iGTSW[0];
                await ReplyAsync("", false, new EmbedBuilder
                {
                    Title = "Webhook deleted successfully!",
                    Description = $"The webhook `{reqwh.Name}` of channel <#{reqwh.ChannelId}> was deleted successfully!!",
                    Color = Blurple
                }.WithCurrentTimestamp());
                await reqwh.DeleteAsync();
                return;
            }
            var emb = new EmbedBuilder
            {
                Title = "Multiple Webhooks found!",
                Description = $"We found multiple webhooks by your query `{args[0]}`",
                Color = Color.Red
            }.WithCurrentTimestamp();
            for (int i = 0; i < iGTSW.Count; i++)
            {
                Discord.Rest.RestWebhook rw = iGTSW[i];
                emb.AddField($"{i + 1}) " + rw.Name, $"Channel: <#{rw.ChannelId}>\nCreated By: {rw.Creator.Username}#{rw.Creator.Discriminator}\nAvatar: [link]({(string.IsNullOrEmpty(rw.GetAvatarUrl()) ? "https://discord.com/assets/6debd47ed13483642cf09e832ed0bc1b.png" : rw.GetAvatarUrl())})");
            }
            await ReplyAsync("", false, emb);
            return;
        }
    }
}