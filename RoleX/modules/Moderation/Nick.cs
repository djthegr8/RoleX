using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using RoleX.Modules.Services;

namespace RoleX.Modules.Moderation
{
    [DiscordCommandClass("Moderation", "Basic Moderation for yer server!")]
    public class Nick : CommandModuleBase
    {
        [RequiredUserPermissions(GuildPermission.ManageNicknames)]
        [DiscordCommand("nick", commandHelp = "nick @User <multi-word-string>", example ="nick @DJ001 Weird Dumbass")]
        [Alt("nickname")]
        public async Task RNick(params string[] args)
        {
            if (args.Length == 0 || args.Length == 1)
            {
                await ReplyAsync("", false, new EmbedBuilder
                {
                    Title = "Insufficient Parameters!",
                    Description = $"The way to use the command is \n`{await SqliteClass.PrefixGetter(Context.Guild.Id)}nick <@User> <new-user-nickname>`",
                    Color = Color.Red
                }.WithCurrentTimestamp());
                return;
            }
            if (GetUser(args[0]) == null)
            {
                await ReplyAsync("", false, new EmbedBuilder
                {
                    Title = "Invalid user",
                    Description = $"`{args[0]}` could not be parsed as user!",
                    Color = Color.Red
                }.WithCurrentTimestamp());
                return;
            }
            var bchname = string.Join(' ', args.Skip(1));
            if (bchname.Length < 0 || bchname.Length > 32)
            {
                await ReplyAsync("", false, new EmbedBuilder
                {
                    Title = "Invalid user nickname",
                    Description = $"`{bchname}` is an invalid name, as it either ~ \n1) Contains non-allowed characters\n 2) Is too long",
                    Color = Color.Red
                }.WithCurrentTimestamp());
                return;
            }
            var cha = await GetUser(args[0]);
            if (cha.Hierarchy >= (Context.User as SocketGuildUser).Hierarchy && cha.Id != Context.User.Id)
            {
                await ReplyAsync("", false, new EmbedBuilder
                {
                    Title = "They are above you",
                    Description = $"Respect 'em, dont change their nick smh",
                    Color = Color.Red
                }.WithCurrentTimestamp());
                return;
            }
            if (cha.Hierarchy >= Context.Guild.CurrentUser.Hierarchy)
            {
                await ReplyAsync("", false, new EmbedBuilder
                {
                    Title = "Oops, that person is above me :(",
                    Description = $"I don't have sufficient permissions to ban them",
                    Color = Color.Red
                }.WithCurrentTimestamp());
                return;
            }
            await cha.ModifyAsync(i => i.Nickname = bchname);
            await ReplyAsync("", false, new EmbedBuilder
            {
                Title = "Nickname Updated!!",
                Description = $"<@{cha.Id}> is now set!!!",
                Color = Blurple
            }.WithCurrentTimestamp());
            return;
        }
    }
}