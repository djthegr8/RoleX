using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord.WebSocket;
using Discord;
namespace RoleX.Modules.Services
{
    public class User
    {
        /// <summary>
        /// Similar to <see cref="PremiumType"/> with additional Unknown
        /// </summary>
        public enum NitroTypes
        {
            Unknown,
            Nitro,
            Classic,
            None
        }
        /// <summary>
        /// ID of the <see cref="SocketUser"/>
        /// </summary>
        public ulong ID { get; set; }
        /// <summary>
        /// Type of Nitro the uses possesses
        /// </summary>
        public NitroTypes NitroType { get; set; } = NitroTypes.Unknown;
        /// <summary>
        /// List of strings with all the things this person wants to buy
        /// </summary>
        public List<string> BuyingString { get; set; } = new();
        /// <summary>
        /// List of strings with all the things this person wants to sell
        /// </summary>
        public List<string> SellingString { get; set; } = new();
        /// <summary>
        /// List of tracked users' IDs
        /// </summary>
        public List<ulong> TrackedUserIDs { get; set; } = new();
        /// <summary>
        /// List of reminders this person has
        /// </summary>
        public List<SqliteClass.Reminder> Reminders { get; set; } = new();

    }
}
