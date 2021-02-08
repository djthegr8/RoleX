using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Discord.WebSocket;
using Microsoft.Data.Sqlite;
using P = RoleX.Modules.Services.Punishment;

namespace RoleX.Modules.Services
{
    public class SqliteClass
    {
        /// <summary>
        /// A class for a Reaction Role.
        /// </summary>
        public class ReactRole
        {
            /// <summary>
            /// ID of the <see cref="SocketTextChannel"/> where the react role message is present.
            /// </summary>
            public ulong ChannelId { get; set; }
            /// <summary>
            /// ID of the Message with the Reaction Roles
            /// </summary>
            public ulong MessageId { get; set; }
            /// <summary>
            /// ID of <see cref="SocketGuild"/>
            /// </summary>
            public ulong GuildId { get; set; }
            /// <summary>
            /// Whether a person can pick up just one role.
            /// </summary>
            public bool Unique { get; set; } = false;
            /// <summary>
            /// <see cref="Array"/> of Roles that can be picked up in the Reaction Role.
            /// </summary>
            public List<ulong> Roles { get; set; } = new List<ulong>();
            /// <summary>
            /// <see cref="Array"/> of emojis that can be picked up in the Reaction role.
            /// </summary>
            public List<string> Emojis { get; set; } = new List<string>();
            /// <summary>
            /// <see cref="Array"/> of Whitelisted roles, i.e., roles that allow people to pick up the Reaction Roles
            /// </summary>
            public ulong[] WhiteListedRoles { get; set; } = new ulong[] { };
            /// <summary>
            /// <see cref="Array"/> of Blacklisted roles, i.e., roles disallowing people to pick up the Reaction Roles.
            /// </summary>
            public ulong[] BlackListedRoles { get; set; } = new ulong[] { };
            /// <summary>
            /// The Reaction role self destructs at the given time, if not set then equals <see cref="DateTime.MinValue"/>.<br /> <a href="https://docs.carl.gg/roles/reaction-roles/#rr-management">See here for info</a>
            /// </summary>
            public DateTime SelfDestructTime { get; set; } = new DateTime();
            public ReactRole()
            {

            }
        }

        public enum TradeTexts
        {
            Buying,
            Selling
        }
        public class Reminder {
            /// <summary>
            /// The Reminder ID.
            /// </summary>
            public string Id { get; set; } = Guid.NewGuid().ToString();
            /// <summary>
            /// ID of User who set the Reminder 
            /// </summary>
            public ulong UserId { get; set; }
            /// <summary>
            /// Time when reminder to DM!
            /// </summary>
            public DateTime TimeS { get; internal set; }
            public string Time { set { TimeS = DateTime.Parse(value); } }
            /// <summary>
            /// Reason why reminder was set, or "Not given"
            /// </summary>
            public string Reason { get; set; } = "Not given";
            /// <summary>
            /// Empty Constructor for usage
            /// </summary>
            public Reminder() { }
        }
        public class Infraction
        {
            public ulong GuildId { get; set; }
            public ulong ModeratorId { get; set; }
            public ulong UserId { get; set; }

            public P Punishment { get; set; }

            public static P SetPunishment(string value)
            {
                return value switch
                {
                    "Ban" => P.Ban,
                    "Mute" => P.Mute,
                    "Kick" => P.Kick,
                    "HardMute" => P.HardMute,
                    "Softban" => P.Softban,
                    "Unban" => P.Unban,
                    _ => throw new NotImplementedException("Irdk what happened :/")
                };
            }
            public static string GetPunishment(P pment)
            {
                return pment switch
                {
                    P.Ban => "Ban",
                    P.Mute => "Mute",
                    P.Kick => "Kick",
                    P.HardMute => "HardMute",
                    P.Softban => "Softban",
                    P.Unban => "Unban",
                    _ => "kekw i hate the compiler"
                };
            }
            public DateTime Time { get; set; }
            public static DateTime SetDTime(string dTaime)
            {
                return DateTime.Parse(dTaime);
            }
            public string Reason { get; set; }
            public Infraction() { }
        }
        public class Invite
        {
            public ulong GuildId { get; set; }
            public ulong CreatorId { get; set; }
            public DateTime Created { get; set; }
            public short JoinedNum { get; set; }
            public string Code { get; set; }
            public Invite() { }
        }
        /*public static SqliteCommand Connect()
        {
            
        }*/
        private const string FilePath = "Data Source = ../Data/rolex.db";

        /// <summary>
        /// Creates a Reader (Query) function skeleton
        /// </summary>
        /// <typeparam name="T">The return SQLite Dtype</typeparam>
        /// <param name="cmdtext">Command text to execute in SQLite</param>
        /// <param name="defval">Default value if no rows are found</param>
        /// <returns>The query reply (if exists) or default value</returns>
        private static async Task<T> QueryFunctionCreator<T>(string cmdtext, T defval)
        {
            await using var con = new SqliteConnection(FilePath);
            await con.OpenAsync();
            await using var cmd = new SqliteCommand
            {
                Connection = con,
                CommandText = cmdtext
            };
            var read = await cmd.ExecuteReaderAsync();
            await read.ReadAsync();
            T retval;
            if (!read.HasRows || await read.IsDBNullAsync(0)) retval = defval;
            else
            {
                retval = (T)read[0];
            }
            await read.CloseAsync();
            await con.CloseAsync();
            return retval;
        }
        public static async Task ReminderFinished(Reminder rmdr) => await NonQueryFunctionCreator($"UPDATE reminders SET Finished = 1 WHERE ID = \"{rmdr.Id}\";");
        public static async Task AddReminder(Reminder rmdr) => await NonQueryFunctionCreator($"INSERT INTO reminders VALUES (\"{rmdr.Id}\",{rmdr.UserId}, \"{rmdr.TimeS:u}\", \"{rmdr.Reason}\", 0);");
        /// <summary>
        /// Gets all reminders meeting pattern. Note that Finished? isn't a property.
        /// </summary>
        /// <param name="cmdtext">The text to execute</param>
        /// <returns>List of reminders meeting <paramref name="cmdtext"/></returns>
        public static async Task<List<Reminder>> GetReminders(string cmdtext)
        {
            List<Reminder> retvals = new List<Reminder>();
            await using var con = new SqliteConnection(FilePath);
            await con.OpenAsync();
            await using var cmd = new SqliteCommand
            {
                Connection = con,
                CommandText = cmdtext
            };
            var read = await cmd.ExecuteReaderAsync();
            await read.ReadAsync();
            if (!read.HasRows || await read.IsDBNullAsync(0)) return new List<Reminder>();
            do
            {
                retvals.Add(new Reminder
                {
                    Id = read.GetString(0),
                    UserId = Convert.ToUInt64(read.GetInt64(1)),
                    Time = read.GetString(2),
                    Reason = read.GetString(3)
                });
            } while (await read.ReadAsync());
            await read.CloseAsync();
            await con.CloseAsync();
            return retvals;
        }
        /// <summary>
        /// Creates a MULTI LINE Reader (Query) function skeleton
        /// </summary>
        /// <param name="cmdtext">Command text to execute in SQLite</param>
        /// <returns>The query reply (if exists) or default value</returns>
        public static async Task<List<Infraction>> GetInfractions(string cmdtext)
        {
            List<Infraction> retvals = new List<Infraction>();
            await using var con = new SqliteConnection(FilePath);
            await con.OpenAsync();
            await using var cmd = new SqliteCommand
            {
                Connection = con,
                CommandText = cmdtext
            };
            var read = await cmd.ExecuteReaderAsync();
            await read.ReadAsync();
            if (!read.HasRows || await read.IsDBNullAsync(0)) return new List<Infraction>();
            do
            {
                retvals.Add(new Infraction
                {
                    GuildId = Convert.ToUInt64(read.GetInt64(0)),
                    UserId = Convert.ToUInt64(read.GetInt64(1)),
                    ModeratorId = Convert.ToUInt64(read.GetInt64(2)),
                    Punishment = Infraction.SetPunishment(read.GetString(3)),
                    Time = Infraction.SetDTime(read.GetString(4)),
                    Reason = read.GetString(5)
                });
            } while (await read.ReadAsync());
            await read.CloseAsync();
            await con.CloseAsync();
            return retvals;
        }
        public static async Task AddOrUpdateReactRole(ReactRole rrl)
        {
            await NonQueryFunctionCreator($"REPLACE INTO reactroles VALUES ({rrl.ChannelId}, {rrl.MessageId}, {rrl.GuildId}, {Convert.ToInt32(rrl.Unique)}, \"{string.Join(',', rrl.Emojis)}\", \"{string.Join(',', rrl.Roles.Select(x => $"{x}"))}\", \"{string.Join(',', rrl.BlackListedRoles.Select(x => $"{x}"))}\", \"{string.Join(',', rrl.WhiteListedRoles.Select(x => $"{x}"))}\", \"{rrl.SelfDestructTime:u}\");");
        }
        /// <summary>
        /// Gets the react role(s) 
        /// </summary>
        /// <param name="cmdtext">Text for SQLite Query</param>
        /// <returns>Returns the react role or empty <see cref="List{ReactRole}"/> if none are found.</returns>
        public static async Task<List<ReactRole>> GetReactRoleAsync(string cmdtext)
        {
            List<ReactRole> rl = new();
            await using var con = new SqliteConnection(FilePath);
            await con.OpenAsync();
            await using var cmd = new SqliteCommand
            {
                Connection = con,
                CommandText = cmdtext
            };
            var read = await cmd.ExecuteReaderAsync();
            await read.ReadAsync();
            if (!read.HasRows || await read.IsDBNullAsync(0)) return new List<ReactRole>();
            do {
                rl.Add(
                    new ReactRole
                    {
                        ChannelId = ulong.Parse(read.GetInt64(0).ToString()),
                        MessageId = ulong.Parse(read.GetInt64(1).ToString()),
                        GuildId = ulong.Parse(read.GetInt64(2).ToString()),
                        Unique = read.GetInt32(3) == 1,
                        Emojis = read.GetString(4).Split(',').ToList(),
                        Roles = read.GetString(5).Split(',').Select(x => ulong.Parse(x)).ToList(),
                        BlackListedRoles = read.GetString(6) == "" ? new ulong[] { } : read.GetString(6).Split(',').Select(x => ulong.Parse(x)).ToArray(),
                        WhiteListedRoles = read.GetString(7) == "" ? new ulong[] { } : read.GetString(7).Split(',').Select(x => ulong.Parse(x)).ToArray(),
                        SelfDestructTime = DateTime.Parse(read.GetString(8))
                    }
                    );
            } while (await read.ReadAsync());
            await read.CloseAsync();
            await con.CloseAsync();
            return rl;
        }
        /// <summary>
        /// Creates a skeleton NonQuery function
        /// </summary>
        /// <param name="cmdtext">The command text to execute in SQLite</param>
        /// <returns></returns>
        public static async Task NonQueryFunctionCreator(string cmdtext)
        {
            await using var con = new SqliteConnection(FilePath);
            await con.OpenAsync();
            await using var cmd = new SqliteCommand
            {
                Connection = con,
                CommandText = cmdtext
            };
            var rds = await cmd.ExecuteNonQueryAsync();
            if (rds == 0)
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.BackgroundColor = ConsoleColor.DarkRed;
            }
            Console.WriteLine(rds != 0 ? "Successful SQLite NonQuery" : "Unsuccessful SQLite NonQuery");
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.BackgroundColor = ConsoleColor.Black;
            await con.CloseAsync();
        }
        // All getters
        /// <summary>
        /// Gets whether user is on Trade Cooldown
        /// </summary>
        /// <param name="guildId"></param>
        /// <param name="userId"></param>
        /// <returns>A boolean saying whether on Trade Cooldown <c>true</c> if yes, else <c>false</c></returns>
        public static async Task<bool> CooldownGetter(ulong guildId, ulong userId) => await QueryFunctionCreator($"select GuildID from cooldown where guildid = {guildId} and UserID = {userId}", long.Parse("0")) == 0;
        public static async Task<bool> TrackCooldownGetter(ulong userId) => await QueryFunctionCreator($"select count(*) from track_cd where UserID = {userId}", long.Parse("0")) != 0;
        public static async Task<long> TrackCdGetUser(ulong userId) => await QueryFunctionCreator($"select TUserID from track_cd where UserID = {userId}", long.Parse("0"));
        public static async Task<List<ulong>> TrackCdAllUlongIDs(string cmdtext)
        {
            List<ulong> retvals = new List<ulong>();
            await using var con = new SqliteConnection(FilePath);
            await con.OpenAsync();
            await using var cmd = new SqliteCommand
            {
                Connection = con,
                CommandText = cmdtext
            };
            var read = await cmd.ExecuteReaderAsync();
            await read.ReadAsync();
            if (!read.HasRows || await read.IsDBNullAsync(0)) return new List<ulong>();
            do
            {
                retvals.Add(ulong.Parse(read.GetInt64(0).ToString()));
            } while (await read.ReadAsync());
            await read.CloseAsync();
            await con.CloseAsync();
            return retvals;
        }
        /// <summary>
        /// Gets the prefix of the bot
        /// </summary>
        /// <param name="guilId"></param>
        /// <returns></returns>
        public static async Task<string> PrefixGetter(ulong guilId) => await QueryFunctionCreator($"select Prefix from prefixes where guildid = {guilId}", "r");
        public static async Task<string> AppealGetter(ulong guilId) => await QueryFunctionCreator($"select appeal from prefixes where guildid = {guilId}", "");
        public static async Task<long> AltTimePeriodGetter(ulong guildId) => await QueryFunctionCreator($"select AltTimeMonths from prefixes where guildid = {guildId}", long.Parse("3"));
        public static async Task<long> SlowdownTimeGetter(ulong guildId) => await QueryFunctionCreator($"select Slowdown from prefixes where guildid = {guildId}", long.Parse("15"));
        public static async Task<ulong> MutedRoleIdGetter(ulong guildId)
        {
            var ii = await QueryFunctionCreator($"select MutedRoleID from prefixes where guildid = {guildId}", long.Parse("0"));
            return Convert.ToUInt64(ii);
        }
        public static async Task<ulong> AlertChanGetter(ulong guildId)
        {
            var ii = await QueryFunctionCreator($"select AlertChanID from prefixes where GuildID = {guildId}", long.Parse("0"));
            return Convert.ToUInt64(ii);
        }
        public static async Task<ulong> TradingChanGetter(ulong guildId)
        {
            var ii = await QueryFunctionCreator($"select TradingChannel from prefixes where GuildID = {guildId}", long.Parse("0"));
            return Convert.ToUInt64(ii);
        }
        public static async Task<string> StringGetter(ulong userId, TradeTexts tt)
        {
            var bs = tt switch
            {
                TradeTexts.Buying => "BuyingString",
                TradeTexts.Selling => "SellingString",
                _ => ""                // IDE do be bossy
            };
            var st = await QueryFunctionCreator($"select {bs} from tradelists where UserID = {userId}", "");
            if (!st.StartsWith(';') && st != "" && st != ";")
            {
                return ';' + st;
            }
            return st;
        }
        /// <remarks>
        /// Returns new <see cref="List{T}"></see> when none found.
        /// Note that the seperators per-alias is <c>^</c> and for alias and cmd is <c>|</c>
        /// </remarks>
        public static async Task<List<Tuple<string, string>>> GuildAliasGetter(ulong GuildID)
        {
            var aliases = await QueryFunctionCreator($"SELECT aliases FROM alias WHERE GuildID = {GuildID}", "");
            if (aliases == "")
            {
                return new List<Tuple<string, string>>();
            }

            return aliases.Split('^').Select(k => new Tuple<string, string>(k.Split("|")[0], k.Split("|")[1])).ToList();
        }

        public static async Task<bool> PremiumOrNot(ulong GuildID)
        {
            return await QueryFunctionCreator($"SELECT Premium FROM prefixes WHERE GuildID = {GuildID};", long.Parse("0")) == 1;
        }
        // All adders
        public static async Task AliasAdder(ulong GuildID, string aliasName, string aliasContent)
        {
            bool isPremium = await PremiumOrNot(GuildID);
            var CurrentAliases = await GuildAliasGetter(GuildID);
            if ((CurrentAliases.Count >= CommandModuleBase.AllowedAliasesNonPremium && !isPremium) ||
                CurrentAliases.Count >= CommandModuleBase.AllowedAliasesPremium)
            {
                throw new Exception("Alias limit reached.");
            }
            CurrentAliases.Add(new Tuple<string, string>(aliasName, aliasContent));
            var toAddArray = CurrentAliases.Select(k => k.Item1 + "|" + k.Item2);
            if (CurrentAliases.Count() > 1)
                await NonQueryFunctionCreator(
                    $"UPDATE alias SET aliases = \"{string.Join('^', toAddArray)}\" WHERE GuildID = {GuildID}");
            else await NonQueryFunctionCreator($"REPLACE into alias Values({GuildID}, \"{string.Join('^', toAddArray)}\")");
        }

        public static async Task<int> AliasRemover(ulong GuildID, string aliasName)
        {
            var CurrentAliases = await GuildAliasGetter(GuildID);
            var retval = CurrentAliases.RemoveAll(k =>
            {
                Console.WriteLine(aliasName);
                return k.Item1 == aliasName;
            });
            var toAddArray = CurrentAliases.Select(k => k.Item1 + "|" + k.Item2);
            if (!toAddArray.Any()) await NonQueryFunctionCreator($"DELETE FROM alias WHERE GuildID = {GuildID}");
            else await NonQueryFunctionCreator($"UPDATE alias SET aliases = \"{string.Join('^', toAddArray)}\" WHERE GuildID = {GuildID}");
            return retval;
        }
        public static async Task SlowdownTimeAdder(ulong guildId, ulong slowdownTime) => await NonQueryFunctionCreator($"update prefixes set Slowdown = {slowdownTime} where GuildID = {guildId};");
        public static async Task CooldownAdder(ulong guildId, ulong userId) => await NonQueryFunctionCreator($"insert into cooldown (GuildID, UserID) values ({guildId}, {userId});");
        public static async Task CooldownRemover(ulong guildId, ulong userId) => await NonQueryFunctionCreator($"delete from cooldown where GuildID = {guildId} and UserID = {userId};");
        public static async Task Track_AllCDRemover(ulong userId) => await NonQueryFunctionCreator($"delete from track_cd where UserID = {userId};");
        public static async Task Track_CDAdder(ulong userId, ulong otherUserId) => await NonQueryFunctionCreator($"insert into track_cd (UserID, TUserID) values ({userId}, {otherUserId});");
        public static async Task Track_CDRemover(ulong userId, ulong otherUserId) => await NonQueryFunctionCreator($"delete from track_cd where UserID = {userId} and TUserID = {otherUserId};");
        public static async Task MutedRoleIdAdder(ulong guildId, ulong mutedRoleId) => await NonQueryFunctionCreator($"update prefixes set MutedRoleID = {mutedRoleId} where GuildID = {guildId};");
        public static async Task PrefixAdder(ulong guLdid, string prefix)
        {
            await NonQueryFunctionCreator($"update prefixes set Prefix = \"{prefix}\" where GuildID = {guLdid};");
        }
        public static async Task AppealAdder(ulong guLdid, string appeallink) => await NonQueryFunctionCreator($"update prefixes set appeal = \"{appeallink}\" where GuildID = {guLdid};");
        public static async Task AddToModlogs(ulong guildId, ulong userId, ulong moderatorId, Punishment punishment, DateTime time, string reason = "")
        {
            await NonQueryFunctionCreator($"insert into modlogs (UserID,GuildID,Punishment,ModeratorID,Time{(reason == "" ? "" : ",Reason")}) values ({userId},{guildId},\"{Enum.GetName(typeof(Punishment), punishment)}\",{moderatorId},\"{time:o}\"{(reason == "" ? "" : $",\"{reason}\"")});");
        }
        public static async Task<List<Infraction>> GetUserModlogs(ulong guildId, ulong userId) => await GetInfractions($"select * from modlogs where GuildID = {guildId} and UserID = {userId};");
        public static async Task AlertChanAdder(ulong guildId, ulong chanId) => await NonQueryFunctionCreator($"update prefixes set AlertChanID = {chanId} where GuildID = {guildId};");
        public static async Task TradingChanAdder(ulong guildId, ulong chanId) => await NonQueryFunctionCreator($"update prefixes set TradingChannel = {chanId} where GuildID = {guildId};");
        public static async Task AltTimePeriodAdder(ulong guildId, long altTimeMonths) => await NonQueryFunctionCreator($"update prefixes set AltTimeMonths = {altTimeMonths} where GuildID = {guildId};");
        public static async Task TradeEditor(ulong userId, string text, TradeTexts tt)
        {
            await NonQueryFunctionCreator($"replace into tradelists (UserID, BuyingString, SellingString) values({userId},\"{(tt == TradeTexts.Selling ? await StringGetter(userId, TradeTexts.Buying) : text)}\",\"{(tt == TradeTexts.Buying ? await StringGetter(userId, TradeTexts.Selling) : text)}\");");
        }
    }
}
