using System.Linq;
using System.Threading.Tasks;
using Discord;
using RoleX.Modules.Services;

namespace RoleX.Modules.Channel_Permission
{
    [DiscordCommandClass("Channel Editor", "Edit Channel-wise perms of a Role using these commands!")]
    public class Categoryrename : CommandModuleBase
    {
        [Alt("catre")]
        [Alt("catrename")]
        [RequiredUserPermissions(GuildPermission.ManageChannels, GuildPermission.ManageGuild)]
        [DiscordCommand("categoryrename", commandHelp = "categoryrename <old-category-name> <new-category-name>", description = "Renames given category", example = "categoryrename Trading Xtreme Trading")]
        public async Task CatRename(params string[] args)
        {
            if (args.Length < 2)
            {
                await ReplyAsync("", false, new EmbedBuilder
                {
                    Title = "Insufficient Parameters!",
                    Description = $"The way to use the command is \n`{await SqliteClass.PrefixGetter(Context.Guild.Id)}categoryrename <old-category-name> <new-category-name>`",
                    Color = Color.Red
                }.WithCurrentTimestamp());
                return;
            }
            var alf = GetCategory(args[0]);
            if (alf == null)
            {
                await ReplyAsync("", false, new EmbedBuilder
                {
                    Title = "Invalid category",
                    Description = $"`{args[0]}` could not be parsed as category!",
                    Color = Color.Red
                }.WithCurrentTimestamp());
                return;
            }
            await alf.ModifyAsync(x => x.Name = string.Join(' ', args.Skip(1)));
            await ReplyAsync("", false, new EmbedBuilder
            {
                Title = "Rename successful!",
                Description = $"Your category was renamed to `{string.Join(' ', args.Skip(1))}`",
                Color = Blurple
            }.WithCurrentTimestamp());
            return;
        }
    }
}