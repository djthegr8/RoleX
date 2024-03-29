﻿using System.Threading.Tasks;
using DiscordBotsList.Api;
using DiscordBotsList.Api.Objects;

namespace Hermes.Modules.Services
{
    internal class TopGG
    {
        public static AuthDiscordBotListApi DblApi =
            new(744766526225252435,
                "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpZCI6Ijc0NDc2NjUyNjIyNTI1MjQzNSIsImJvdCI6dHJ1ZSwiaWF0IjoxNjA4NTUzMjA5fQ.ShvKIo4A2rVXpLEJVUjsobUzdfMg4-B3o7qLvz8aROg");

        public static async Task<bool> HasVoted(ulong ide)
        {
            IDblBot me = await DblApi.GetMeAsync();
            return await DblApi.HasVoted(ide);
        }

        public static async Task topGGUPD(int gc)
        {
            var me = await DblApi.GetMeAsync();
            await me.UpdateStatsAsync(gc);
        }
    }
}