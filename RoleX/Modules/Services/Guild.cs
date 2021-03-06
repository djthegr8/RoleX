using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord.WebSocket;
namespace RoleX.Modules.Services
{
    public class Guild
    {
        /// <summary>
        /// ID of the <see cref="SocketGuild"/>
        /// </summary>
        public ulong GuildID { get; set; }
        /// <summary>
        /// Prefix to be used with RoleX
        /// </summary>
        public string Prefix { get; set; } = "r";
        /// <summary>
        /// URL for appeals
        /// </summary>
        public string AppealUrl { get; set; }
        /// <summary>
        /// ID of the Muted Role
        /// </summary>
        public ulong MutedRoleID { get; set; }
        /// <summary>
        /// ID of the Alert Channel
        /// </summary>
        public ulong AlertChannelID { get; set; }
        /// <summary>
        /// ID of the Trading Channel
        /// </summary>
        public ulong TradingChannelID { get; set; }
        /// <summary>
        /// Number of months old accounts to be flagged as alt, at maximum
        /// </summary>
        public ushort AltTimeMonths { get; set; }
        /// <summary>
        /// Slowmode in the trades allowed
        /// </summary>
        public ushort TradingSlowdownHrs { get; set; }
        /// <summary>
        /// Whether the guild is premium
        /// </summary>
        public bool Premium { get; set; } = false;
        /// <summary>
        /// List of Guild Aliases
        /// </summary>
        public List<Tuple<string, string>> Aliases { get; set; } = new();

    }
}
