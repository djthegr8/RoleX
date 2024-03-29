﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord.WebSocket;
using Microsoft.Data.Sqlite;
using Newtonsoft.Json;
using P = Hermes.Modules.Services.Punishment;

namespace Hermes.Modules.Services
{
    public partial class SqliteClass
    {
        public enum TradeTexts
        {
            Buying,
            Selling
        }

        /*public static SqliteCommand Connect()
        {
            
        }*/
        private const string FilePath = "Data Source = ../Data/rolex.db";

        /// <summary>
        ///     Creates a Reader (Query) function skeleton
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
                retval = (T) read[0];
            await read.CloseAsync();
            await con.CloseAsync();
            return retval;
        }

        public static async Task ReminderFinished(Reminder rmdr)
        {
            await NonQueryFunctionCreator($"UPDATE reminders SET Finished = 1 WHERE ID = \"{rmdr.Id}\";");
        }

        public static async Task AddReminder(Reminder rmdr)
        {
            await NonQueryFunctionCreator(
                $"INSERT INTO reminders VALUES (\"{rmdr.Id}\",{rmdr.UserId}, \"{rmdr.TimeS:u}\", \"{rmdr.Reason}\", 0);");
        }

        /// <summary>
        ///     Gets all reminders meeting pattern. Note that Finished? isn't a property.
        /// </summary>
        /// <param name="cmdtext">The text to execute</param>
        /// <returns>List of reminders meeting <paramref name="cmdtext" /></returns>
        public static async Task<List<Reminder>> GetReminders(string cmdtext)
        {
            var retvals = new List<Reminder>();
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
                if (read.GetString(2) == "") continue;
                retvals.Add(new Reminder
                {
                    Id = read.GetString(0),
                    UserId = Convert.ToUInt64(read.GetInt64(1)),
                    Time = read.GetString(2),
                    Reason = read.GetString(3),
                    Finished = read.GetInt32(4) == 1
                });
            } while (await read.ReadAsync());

            await read.CloseAsync();
            await con.CloseAsync();
            return retvals;
        }

        /// <summary>
        ///     Creates a MULTI LINE Reader (Query) function skeleton
        /// </summary>
        /// <param name="cmdtext">Command text to execute in SQLite</param>
        /// <returns>The query reply (if exists) or default value</returns>
        public static async Task<List<Infraction>> GetInfractions(string cmdtext)
        {
            var retvals = new List<Infraction>();
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
            await NonQueryFunctionCreator(
                $"REPLACE INTO reactroles VALUES ({rrl.ChannelId}, {rrl.MessageId}, {rrl.GuildId}, {Convert.ToInt32(rrl.Unique)}, \"{string.Join(',', rrl.Emojis)}\", \"{string.Join(',', rrl.Roles.Select(x => $"{x}"))}\", \"{string.Join(',', rrl.BlackListedRoles.Select(x => $"{x}"))}\", \"{string.Join(',', rrl.WhiteListedRoles.Select(x => $"{x}"))}\", \"{rrl.SelfDestructTime:u}\");");
        }

        /// <summary>
        ///     Gets the react role(s)
        /// </summary>
        /// <param name="cmdtext">Text for SQLite Query</param>
        /// <returns>Returns the react role or empty <see cref="List{ReactRole}" /> if none are found.</returns>
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
            do
            {
                rl.Add(
                    new ReactRole
                    {
                        ChannelId = ulong.Parse(read.GetInt64(0).ToString()),
                        MessageId = ulong.Parse(read.GetInt64(1).ToString()),
                        GuildId = ulong.Parse(read.GetInt64(2).ToString()),
                        Unique = read.GetInt32(3) == 1,
                        Emojis = read.GetString(4).Split(',').ToList(),
                        Roles = read.GetString(5).Split(',').Select(x => ulong.Parse(x)).ToList(),
                        BlackListedRoles = read.GetString(6) == ""
                            ? new ulong[] { }
                            : read.GetString(6).Split(',').Select(x => ulong.Parse(x)).ToArray(),
                        WhiteListedRoles = read.GetString(7) == ""
                            ? new ulong[] { }
                            : read.GetString(7).Split(',').Select(x => ulong.Parse(x)).ToArray(),
                        SelfDestructTime = DateTime.Parse(read.GetString(8))
                    }
                );
            } while (await read.ReadAsync());

            await read.CloseAsync();
            await con.CloseAsync();
            return rl;
        }

        /// <summary>
        ///     Creates a skeleton NonQuery function
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
        /*
        public static async Task<List<Giveaway>> GetGiveaways(string cmdtext)
        {
            List<Giveaway> rl = new();
            await using var con = new SqliteConnection(FilePath);
            await con.OpenAsync();
            await using var cmd = new SqliteCommand
            {
                Connection = con,
                CommandText = cmdtext
            };
            var read = await cmd.ExecuteReaderAsync();
            await read.ReadAsync();
            if (!read.HasRows || await read.IsDBNullAsync(0)) return new List<Giveaway>();
            do
            {
                rl.Add(
                    new Giveaway()
                    {
                        GuildID = ulong.Parse(read.GetInt64(0).ToString()),
                        ChannelID = ulong.Parse(read.GetInt64(1).ToString()),
                        MessageID = ulong.Parse(read.GetInt64(2).ToString()),
                        StarterID = ulong.Parse(read.GetInt64(3).ToString()),
                        Winners = read.GetInt32(4),
                        RoleRequirementString = read.GetString(5),
                        Title = read.GetString(6),
                        EndingTime = DateTime.Parse(read.GetString(7)),
                        AmariLevelRequirement = read.GetInt32(8),
                        WeeklyAmariRequirement = read.GetInt32(9),
                        NoNitroReq = read.GetInt32(10) == 1
                    }
                );
            } while (await read.ReadAsync());
            await read.CloseAsync();
            await con.CloseAsync();
            return rl;
        }
        */
        /// <summary>
        ///     Gets whether user is on Trade Cooldown
        /// </summary>
        /// <param name="guildId"></param>
        /// <param name="userId"></param>
        /// <returns>A boolean saying whether on Trade Cooldown <c>true</c> if yes, else <c>false</c></returns>
        public static async Task<bool> CooldownGetter(ulong guildId, ulong userId)
        {
            return await QueryFunctionCreator(
                $"select count(*) from cooldown where guildid = {guildId} and UserID = {userId}", long.Parse("0")) == 1;
        }

        public static async Task<bool> TrackCooldownGetter(ulong userId)
        {
            return await QueryFunctionCreator($"select count(*) from track_cd where UserID = {userId}",
                long.Parse("0")) != 0;
        }

        public static async Task<long> TrackCdGetUser(ulong userId)
        {
            return await QueryFunctionCreator($"select TUserID from track_cd where UserID = {userId}", long.Parse("0"));
        }

        public static async Task<List<ulong>> TrackCdAllUlongIDs(string cmdtext)
        {
            var retvals = new List<ulong>();
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
        ///     Gets the prefix of the bot
        /// </summary>
        /// <param name="guilId"></param>
        /// <returns></returns>
        public static async Task<string> PrefixGetter(ulong guilId)
        {
            return await QueryFunctionCreator($"select Prefix from prefixes where guildid = {guilId}", "r");
        }

        public static async Task<string> AppealGetter(ulong guilId)
        {
            return await QueryFunctionCreator($"select appeal from prefixes where guildid = {guilId}", "");
        }

        public static async Task<long> AltTimePeriodGetter(ulong guildId)
        {
            return await QueryFunctionCreator($"select AltTimeMonths from prefixes where guildid = {guildId}",
                long.Parse("3"));
        }

        public static async Task<long> SlowdownTimeGetter(ulong guildId)
        {
            return await QueryFunctionCreator($"select Slowdown from prefixes where guildid = {guildId}",
                long.Parse("15"));
        }

        public static async Task<ulong> MutedRoleIdGetter(ulong guildId)
        {
            var ii = await QueryFunctionCreator($"select MutedRoleID from prefixes where guildid = {guildId}",
                long.Parse("0"));
            return Convert.ToUInt64(ii);
        }

        public static async Task<ulong> AlertChanGetter(ulong guildId)
        {
            var ii = await QueryFunctionCreator($"select AlertChanID from prefixes where GuildID = {guildId}",
                long.Parse("0"));
            return Convert.ToUInt64(ii);
        }

        public static async Task<ulong> TradingChanGetter(ulong guildId)
        {
            var ii = await QueryFunctionCreator($"select TradingChannel from prefixes where GuildID = {guildId}",
                long.Parse("0"));
            return Convert.ToUInt64(ii);
        }

        public static async Task<string> StringGetter(ulong userId, TradeTexts tt)
        {
            var bs = tt switch
            {
                TradeTexts.Buying => "BuyingString",
                TradeTexts.Selling => "SellingString",
                _ => "" // IDE do be bossy
            };
            var st = await QueryFunctionCreator($"select {bs} from tradelists where UserID = {userId}", "");
            if (!st.StartsWith(';') && st != "" && st != ";") return ';' + st;
            return st;
        }

        /// <remarks>
        ///     Returns new <see cref="List{T}"></see> when none found.
        ///     Note that the seperators per-alias is <c>^</c> and for alias and cmd is <c>|</c>
        /// </remarks>
        public static async Task<List<Tuple<string, string>>> GuildAliasGetter(ulong GuildID)
        {
            var aliases = await QueryFunctionCreator($"SELECT aliases FROM alias WHERE GuildID = {GuildID}", "");
            if (aliases == "") return new List<Tuple<string, string>>();

            return aliases.Split('^').Select(k => new Tuple<string, string>(k.Split("|")[0], k.Split("|")[1])).ToList();
        }

        public static async Task<bool> PremiumOrNot(ulong GuildID)
        {
            return await QueryFunctionCreator($"SELECT Premium FROM prefixes WHERE GuildID = {GuildID};",
                long.Parse("0")) == 1;
        }

        // All adders
        public static async Task AliasAdder(ulong GuildID, string aliasName, string aliasContent)
        {
            var isPremium = await PremiumOrNot(GuildID);
            var CurrentAliases = await GuildAliasGetter(GuildID);
            if (CurrentAliases.Count >= CommandModuleBase.AllowedAliasesNonPremium && !isPremium ||
                CurrentAliases.Count >= CommandModuleBase.AllowedAliasesPremium)
                throw new Exception("Alias limit reached.");
            CurrentAliases.Add(new Tuple<string, string>(aliasName, aliasContent));
            var toAddArray = CurrentAliases.Select(k => k.Item1 + "|" + k.Item2);
            var strImUsing = $"REPLACE into alias Values({GuildID}, \"{string.Join('^', toAddArray)}\")";
            Console.WriteLine(strImUsing);
            await NonQueryFunctionCreator(strImUsing);
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
            else
                await NonQueryFunctionCreator(
                    $"UPDATE alias SET aliases = \"{string.Join('^', toAddArray)}\" WHERE GuildID = {GuildID}");
            return retval;
        }

        public static async Task AddChartStr(List<Tuple<string, ulong>> info, DateTime lastMessage, ulong ChanneLID)
        {
            var srs = JsonConvert.SerializeObject(info);
            await NonQueryFunctionCreator($"REPLACE INTO messages VALUES({ChanneLID},{srs},{lastMessage:u});");
        }

        public static async Task SlowdownTimeAdder(ulong guildId, ulong slowdownTime)
        {
            await NonQueryFunctionCreator(
                $"INSERT INTO prefixes(guildid,Slowdown) Values({guildId},{slowdownTime}) ON CONFLICT(guildid) DO UPDATE SET Slowdown = {slowdownTime};");
        }

        public static async Task CooldownAdder(ulong guildId, ulong userId)
        {
            await NonQueryFunctionCreator($"insert into cooldown (GuildID, UserID) values ({guildId}, {userId});");
        }

        public static async Task CooldownRemover(ulong guildId, ulong userId)
        {
            await NonQueryFunctionCreator($"delete from cooldown where GuildID = {guildId} and UserID = {userId};");
        }

        public static async Task Track_AllCDRemover(ulong userId)
        {
            await NonQueryFunctionCreator($"delete from track_cd where UserID = {userId};");
        }

        public static async Task Track_CDAdder(ulong userId, ulong otherUserId)
        {
            await NonQueryFunctionCreator($"insert into track_cd (UserID, TUserID) values ({userId}, {otherUserId});");
        }

        public static async Task Track_CDRemover(ulong userId, ulong otherUserId)
        {
            await NonQueryFunctionCreator($"delete from track_cd where UserID = {userId} and TUserID = {otherUserId};");
        }

        public static async Task MutedRoleIdAdder(ulong guildId, ulong mutedRoleId)
        {
            await NonQueryFunctionCreator(
                $"INSERT INTO prefixes(guildid, MutedRoleID) Values({guildId}, {mutedRoleId}) ON CONFLICT(guildid) DO UPDATE SET MutedRoleID = {mutedRoleId}");
        }

        public static async Task PrefixAdder(ulong guLdid, string prefix)
        {
            await NonQueryFunctionCreator(
                $"INSERT INTO prefixes(guildId,Prefix) Values({guLdid},\"{prefix}\") ON CONFLICT(guildId) DO UPDATE SET Prefix = \"{prefix}\";");
        }

        public static async Task AppealAdder(ulong guLdid, string appeallink)
        {
            await NonQueryFunctionCreator(
                $"INSERT INTO prefixes(guildid,appeal) Values({guLdid},\"{appeallink}\") ON CONFLICT(guildid) DO UPDATE SET appeal = \"{appeallink}\";");
        }

        public static async Task AddToModlogs(ulong guildId, ulong userId, ulong moderatorId, Punishment punishment,
            DateTime time, string reason = "")
        {
            await NonQueryFunctionCreator(
                $"insert into modlogs (UserID,GuildID,Punishment,ModeratorID,Time{(reason == "" ? "" : ",Reason")}) values ({userId},{guildId},\"{Enum.GetName(typeof(Punishment), punishment)}\",{moderatorId},\"{time:O}\"{(reason == "" ? "" : $",\"{reason}\"")});");
        }

        /// <summary>
        /// Removes giveaway (sets as Inactive)
        /// </summary>
        /// <param name="gaw">The giveaway whose time has runout/has been ended</param>
        /// <returns></returns>
        /*public static async Task GiveawayRemover(Giveaway gaw)
        {
            await NonQueryFunctionCreator(
                $"UPDATE giveaways SET Running = 0 WHERE GuildID = {gaw.GuildID} AND ChannelID = {gaw.ChannelID} AND MessageID = {gaw.MessageID};");
        }*/
        /// <summary>
        ///     Adds a giveaway
        /// </summary>
        /// <param name="addGiveaway">The giveaway to add</param>
        /// <returns></returns>
        /*public static async Task GiveawayAdder(Giveaway addGiveaway)
        {
            await NonQueryFunctionCreator($"REPLACE INTO giveaways VALUES({addGiveaway.GuildID}, {addGiveaway.ChannelID}, {addGiveaway.MessageID}, {addGiveaway.StarterID}, {addGiveaway.Winners}, \"{addGiveaway.RoleRequirementString}\", \"{addGiveaway.Title}\", \"{addGiveaway.EndingTime:u}\", {addGiveaway.AmariLevelRequirement}, {addGiveaway.WeeklyAmariRequirement}, {Convert.ToInt32(addGiveaway.NoNitroReq)}, 1);");
        }*/
        public static async Task<List<Infraction>> GetUserModlogs(ulong guildId, ulong userId)
        {
            return await GetInfractions($"select * from modlogs where GuildID = {guildId} and UserID = {userId};");
        }

        public static async Task AlertChanAdder(ulong guildId, ulong chanId)
        {
            await NonQueryFunctionCreator(
                $"INSERT INTO prefixes(guildid,AlertChanID) Values({guildId},{chanId}) ON CONFLICT(guildid) DO UPDATE SET AlertChanID = {chanId};");
        }

        public static async Task TradingChanAdder(ulong guildId, ulong chanId)
        {
            await NonQueryFunctionCreator(
                $"INSERT INTO prefixes(guildid,TradingChannel) Values({guildId}, {chanId}) ON CONFLICT(guildid) DO UPDATE SET TradingChannel = {chanId};");
        }

        public static async Task AltTimePeriodAdder(ulong guildId, long altTimeMonths)
        {
            await NonQueryFunctionCreator(
                $"INSERT INTO prefixes(guildid,AltTimeMonths) Values({guildId},{altTimeMonths}) ON CONFLICT(guildid) DO UPDATE SET AltTimeMonths = {altTimeMonths};");
        }

        public static async Task TradeEditor(ulong userId, string text, TradeTexts tt)
        {
            await NonQueryFunctionCreator(
                $"replace into tradelists (UserID, BuyingString, SellingString) values({userId},\"{(tt == TradeTexts.Selling ? await StringGetter(userId, TradeTexts.Buying) : text)}\",\"{(tt == TradeTexts.Buying ? await StringGetter(userId, TradeTexts.Selling) : text)}\");");
        }

        /// <summary>
        ///     A class for a Reaction Role.
        /// </summary>
        public class ReactRole
        {
            /// <summary>
            ///     ID of the <see cref="SocketTextChannel" /> where the react role message is present.
            /// </summary>
            public ulong ChannelId { get; set; }

            /// <summary>
            ///     ID of the Message with the Reaction Roles
            /// </summary>
            public ulong MessageId { get; set; }

            /// <summary>
            ///     ID of <see cref="SocketGuild" />
            /// </summary>
            public ulong GuildId { get; set; }

            /// <summary>
            ///     Whether a person can pick up just one role.
            /// </summary>
            public bool Unique { get; set; }

            /// <summary>
            ///     <see cref="Array" /> of Roles that can be picked up in the Reaction Role.
            /// </summary>
            public List<ulong> Roles { get; set; } = new();

            /// <summary>
            ///     <see cref="Array" /> of emojis that can be picked up in the Reaction role.
            /// </summary>
            public List<string> Emojis { get; set; } = new();

            /// <summary>
            ///     <see cref="Array" /> of Whitelisted roles, i.e., roles that allow people to pick up the Reaction Roles
            /// </summary>
            public ulong[] WhiteListedRoles { get; set; } = { };

            /// <summary>
            ///     <see cref="Array" /> of Blacklisted roles, i.e., roles disallowing people to pick up the Reaction Roles.
            /// </summary>
            public ulong[] BlackListedRoles { get; set; } = { };

            /// <summary>
            ///     The Reaction role self destructs at the given time, if not set then equals <see cref="DateTime.MinValue" />.<br />
            ///     <a href="https://docs.carl.gg/roles/reaction-roles/#rr-management">See here for info</a>
            /// </summary>
            public DateTime SelfDestructTime { get; set; }
        }

        public class Reminder
        {
            /// <summary>
            ///     The Reminder ID.
            /// </summary>
            public string Id { get; set; } = Guid.NewGuid().ToString();

            /// <summary>
            ///     ID of User who set the Reminder
            /// </summary>
            public ulong UserId { get; set; }

            /// <summary>
            ///     Time when reminder to DM!
            /// </summary>
            public DateTime TimeS { get; internal set; }

            public string Time
            {
                set => TimeS = DateTime.Parse(value);
            }

            /// <summary>
            ///     Reason why reminder was set, or "Not given"
            /// </summary>
            public string Reason { get; set; } = "Not given";

            public bool Finished { get; set; }
        }

        public class Infraction
        {
            public ulong GuildId { get; set; }
            public ulong ModeratorId { get; set; }
            public ulong UserId { get; set; }

            public Punishment Punishment { get; set; }
            public DateTime Time { get; set; }
            public string Reason { get; set; }

            public static Punishment SetPunishment(string value)
            {
                return value switch
                {
                    "Ban" => Punishment.Ban,
                    "Mute" => Punishment.Mute,
                    "Kick" => Punishment.Kick,
                    "HardMute" => Punishment.HardMute,
                    "Softban" => Punishment.Softban,
                    "Unban" => Punishment.Unban
                };
            }

            public static string GetPunishment(Punishment pment)
            {
                return pment switch
                {
                    Punishment.Ban => "Ban",
                    Punishment.Mute => "Mute",
                    Punishment.Kick => "Kick",
                    Punishment.HardMute => "HardMute",
                    Punishment.Softban => "Softban",
                    Punishment.Unban => "Unban",
                    _ => "kekw i hate the compiler"
                };
            }

            public static DateTime SetDTime(string dTaime)
            {
                return DateTime.Parse(dTaime);
            }
        }
    }
}