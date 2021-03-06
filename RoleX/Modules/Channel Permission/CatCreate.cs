using System.Linq;
using Discord;
using RoleX.Modules.Services;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace RoleX.Modules.Channel_Permission
{
    [DiscordCommandClass("Channel Editor", "Edit channel-wise perms of a role using these commands")]
    internal class CategoryCreate : CommandModuleBase
    {
        [RequiredUserPermissions(GuildPermission.ManageChannels)]
        [Alt("categorycreate")]
        [Alt("catadd")]
        [DiscordCommand("catcreate", commandHelp = "chcreate <name>", description = "Creates category", example = "catcreate weirdo category")]
        public async Task RCreate(params string[] args)
        {
            switch (args.Length)
            {
                case 0:
                    await ReplyAsync("", false, new EmbedBuilder
                    {
                        Title = "Insufficient Parameters",
                        Description =
                            $"The way to use the command is `{await SqliteClass.PrefixGetter(Context.Guild.Id)}catcreate <name>`",
                        Color = Color.Red
                    }.WithCurrentTimestamp());
                    return;
                default:
                {
                    var joined = string.Join(' ', args);
                    var _rchannel = await Context.Guild.CreateCategoryChannelAsync(joined);
                    await ReplyAsync("", false, new EmbedBuilder
                    {
                        Title = "Channel creation successful!",
                        Description = $"Successfully created category `{_rchannel.Name}` (ID: {_rchannel.Id})",
                        Color = Blurple
                    }.WithCurrentTimestamp());
                    break;
                }
            }

        }
    }
}
