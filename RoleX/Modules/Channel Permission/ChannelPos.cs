using System.Threading.Tasks;
using Discord;
using RoleX.Modules.Services;

namespace RoleX.Modules.Channel_Permission
{
    [DiscordCommandClass("Channel Editor", "Edit Channel-wise perms of a Role using these commands!")]
    public class ChannelPos : CommandModuleBase
    {
        [RequiredUserPermissions(GuildPermission.Administrator)]
        [Alt("chpos")]
        [Alt("chposition")]
        
        [DiscordCommand("channelpos", description = "Gets/sets the position of a specified channel", example = "channelpos #general`\n`channelpos #general 3", commandHelp = "channelpos <#channel> (to get) and channelpos <#channel> <position> (to set)")]
        public async Task Cdel(params string[] args)
        {
            if (args.Length == 0) args = new[] {Context.Channel.Id.ToString()};
            var chnl = GetChannel(args[0]);
            var foot = new EmbedFooterBuilder
            {
                Text = "Discord channel positions dont make much sense dont blame me"
            };
            if (args.Length == 1)
            {
                await ReplyAsync(embed:
                    new EmbedBuilder
                    {
                        Title = "Position for given channel",
                        Description = $"The position of <#{chnl.Id}> is `{chnl.Position}`",
                        Color = Blurple,
                        Footer = foot
                    }.WithCurrentTimestamp());
            }
            else
            {
                var fn = int.TryParse(args[1], out var pos);
                if (!fn)
                {
                    await ReplyAsync(embed:
                        new EmbedBuilder
                        {
                            Title = "Invalid Position",
                            Description = $"Couldnt parse {args[1]} as integer",
                            Color = Color.Red
                        }.WithCurrentTimestamp());
                    return;
                }

                await chnl.ModifyAsync(chn => chn.Position = pos);
                await ReplyAsync(embed:
                    new EmbedBuilder
                    {
                        Title = "Position changed",
                        Description = $"The position of <#{chnl.Id}> is now set to `{pos}`",
                        Color = Blurple,
                        Footer = foot
                    }.WithCurrentTimestamp());
            }
        }
    }
}
