using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Hermes.Modules.Services;

namespace Hermes.Modules.Moderation
{
    [DiscordCommandClass("Moderation", "Basic Moderation for yer server!")]
    public class Unlock : CommandModuleBase
    {
        [RequiredUserPermissions(GuildPermission.ManageChannels)]
        [DiscordCommand("unlock", commandHelp = "unlock <#channel>", description = "Unlocks the mentioned channel", example = "unlock #heistchan")]
        public async Task RUnlock(params string[] args)
        {
            SocketGuildChannel lockchnl;
            if (args.Length == 0)
            {
                //Assuming they want to lock the current channel.
                lockchnl = Context.Channel as SocketGuildChannel;
            }
            else
            {
                lockchnl = GetChannel(args[0]);
            }
            if (lockchnl == null)
            {
                await ReplyAsync("", false, new EmbedBuilder
                {
                    Title = "Invalid channel",
                    Description = $"`{args[0]}` could not be parsed as channel!",
                    Color = Color.Red
                }.WithCurrentTimestamp());
                return;
            }
            EmbedBuilder alfa = new EmbedBuilder();
            if (lockchnl is not SocketTextChannel lockMSGchnl)
            {
                var lockVOICE = lockchnl as SocketVoiceChannel;
                var prvo = lockchnl.GetPermissionOverwrite(Context.Guild.EveryoneRole);
                OverwritePermissions xyz;
                if (prvo.HasValue)
                {
                    xyz = prvo.Value.Modify(connect: PermValue.Inherit);
                }
                else
                {
                    xyz = new OverwritePermissions(connect: PermValue.Inherit);
                }
                await lockVOICE.AddPermissionOverwriteAsync(Context.Guild.EveryoneRole, xyz);
                alfa.Title = $"Unlocked Voice Channel {lockVOICE.Name}";
                alfa.Description = "The aforementioned voice channel has been unlocked.";
                alfa.WithCurrentTimestamp();
                alfa.Color = Blurple;
            }
            else
            {
                var prvo = lockchnl.GetPermissionOverwrite(Context.Guild.EveryoneRole);
                OverwritePermissions xyz;
                if (prvo.HasValue)
                {
                    xyz = prvo.Value.Modify(sendMessages: PermValue.Inherit);
                }
                else
                {
                    xyz = new OverwritePermissions(sendMessages: PermValue.Inherit);
                }
                await lockchnl.AddPermissionOverwriteAsync(Context.Guild.EveryoneRole, xyz);
                alfa.Title = $"Unlocked Text Channel {lockMSGchnl.Name}";
                alfa.Description = $"{lockMSGchnl.Mention} has been unlocked";
                alfa.WithCurrentTimestamp();
                alfa.Color = Blurple;
            }
            await Context.Channel.SendMessageAsync("", false, alfa.Build());
        }
    }
}
