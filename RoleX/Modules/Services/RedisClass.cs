using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using StackExchange.Redis;
namespace RoleX.Modules.Services
{
    /// <summary>
    /// Class for Redis Management
    /// </summary>
    public class RedisClass
    {
        private readonly ConnectionMultiplexer muxer = ConnectionMultiplexer.Connect("127.0.0.1:6379,password=rolexDatabase286");
        private static IDatabase conn;
        private const int ServerCooldown = 2;
        public RedisClass()
        { 
            conn = muxer.GetDatabase();
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Started Redis Successful");
            Console.ForegroundColor = ConsoleColor.White;
        }

        private static async Task<bool> SetAsync(string key, string value, TimeSpan expiryTime)
        {
            return await conn.StringSetAsync(key, value, expiryTime);
        }

        private static async Task<string> GetAsync(string key)
        {
            return await conn.StringGetAsync(key);
        }
        /// <summary>
        /// Sets the server cooldown for <see cref="ServerCooldown"/> seconds
        /// </summary>
        /// <param name="GuildID"></param>
        /// <returns></returns>
        public static async Task<bool> SetServerCD(ulong GuildID)
        {
            return await SetAsync(GuildID.ToString(), RedisValue.EmptyString, TimeSpan.FromSeconds(ServerCooldown));

        }
        /// <summary>
        /// Returns true if Server is on Cooldown else false
        /// </summary>
        /// <param name="GuildID"></param>
        /// <returns></returns>
        public static async Task<bool> ServerOnCd(ulong GuildID)
        {
            return await conn.KeyExistsAsync(GuildID.ToString());
        }
    }
}