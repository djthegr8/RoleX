using Discord;
using Discord.WebSocket;
using Public_Bot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoleX.Modules
{
    [DiscordCommandClass("Channel Editor", "Edit Channel-wise perms of a Role using these commands!")]
    public class Pincount : CommandModuleBase
    {
        [Alt("pinc")]
        [DiscordCommand("pincount", description = "Gets the number of pins in the channel", commandHelp = "pincount #channel", example = "pincount #media")]
        public async Task PC(params string[] args)
        {
            if (args.Length == 0)
            {
                args = new string[]{ Context.Channel.Id.ToString()};
            }
            if (GetChannel(args[0]) == null)
            {
                await ReplyAsync("", false, new EmbedBuilder
                {
                    Title = "Invalid channel",
                    Description = $"`{args[0]}` could not be parsed as channel!",
                    Color = Color.Red
                }.WithCurrentTimestamp());
                return;
            }
            var ax = GetChannel(args[0]);
            if (ax as SocketTextChannel == null)
            {
                await ReplyAsync("", false, new EmbedBuilder
                {
                    Title = "Big Brain alert",
                    Description = $"Can u pin messages in a Voice Channel. *Can* you?",
                    Color = Color.Red
                }.WithCurrentTimestamp());
                return;
            }
            var axSTC = ax as SocketTextChannel;
            var pins = (await axSTC.GetPinnedMessagesAsync()).ToList();
            var loa = new List<Tuple<string, int>>();
            foreach (var pin in pins)
            {
                if (loa.Any(i => i.Item1 == pin.Author.Username + "#" + pin.Author.Discriminator))
                {
                    
                }
                else
                {
                    loa.Add(new Tuple<string, int>(pin.Author.Username + "#" + pin.Author.Discriminator, 1));
                }
            }
            loa = loa.Select(x => new Tuple<string, int>(x.Item1, pins.Count(k => k.Author.Username + "#" + k.Author.Discriminator == x.Item1))).ToList();
            loa = loa.OrderByDescending(k => k.Item2).ToList();
            loa = loa.Take(3).ToList();
            await ReplyAsync("", false, new EmbedBuilder
            {
                Title = $"The channel {axSTC.Name} has {pins.Count} pins",
                Description = pins.Count > 1 ? $"Out of these, the top 3 are ~ \n{string.Join('\n', loa.Select((k, l) => $"{( l == 0 ? "🥇" : (l == 1 ? "🥈" : "🥉"))} **{k.Item1}** with {k.Item2} pins"))}" : "No pins eh",
                Color = Blurple
            }.WithCurrentTimestamp());
            return;
        }
    }
}
