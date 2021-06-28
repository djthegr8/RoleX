using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using StackExchange.Redis;

namespace Hermes.Modules.Services
{
    /// <summary>
    /// Class for Redis Management
    /// </summary>
    public class RedisClass
    {
        private static ConnectionMultiplexer muxer;
        private static IDatabase conn;
        private const int ServerCooldown = 2;
        public RedisClass()
        {
            try
            {
                muxer = ConnectionMultiplexer.Connect("127.0.0.1:6379,password=rolexDatabase286");
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Started Redis Successfully");
                Console.ForegroundColor = ConsoleColor.Gray;
            }
            catch
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Starting Redis Failed. Try to start...");
                Console.ForegroundColor = ConsoleColor.Gray;
                Process process = new Process();
                ProcessStartInfo startInfo = new ProcessStartInfo();
                startInfo.WindowStyle = ProcessWindowStyle.Hidden;
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    startInfo.FileName = "bash.exe";
                    startInfo.Arguments = "sudo service redis-server restart";
                }
                else
                {
                    startInfo.FileName = "cmd.exe";
                    startInfo.Arguments = "/C wsl.exe sudo service redis-server restart";
                }
                
                process.StartInfo = startInfo;
                process.Start();
                muxer = ConnectionMultiplexer.Connect("127.0.0.1:6379,password=rolexDatabase286");
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Started Redis Successfully");
                Console.ForegroundColor = ConsoleColor.Gray;
            }
            conn = muxer.GetDatabase();
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
