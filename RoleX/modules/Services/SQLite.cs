﻿using System;
using System.Runtime.CompilerServices;
using static Public_Bot.Punishment;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using Public_Bot;
using System.Collections.Generic;
using System.Linq;
using Discord.Rest;

namespace RoleX.modules
{
    class SqliteClass
    {
        public class Infraction {
            public ulong GuildID { get; set; }
            public ulong ModeratorID { get; set; }
            public ulong UserID { get; set; }

            public Punishment Punishment { get; set; }

            public static Punishment SetPunishment(string value)
            {
                return value switch
                {
                    "Ban" => Ban,
                    "Mute" => Mute,
                    "Kick" => Kick,
                    "HardMute" => HardMute,
                    "Softban" => Softban,
                    "Unban" => Unban,
                    _ => throw new NotImplementedException("Irdk what happened :/")
                };
            }
            public static string GetPunishment(Punishment pment)
            {
                return pment switch
                {
                    Ban => "Ban",
                    Mute => "Mute",
                    Kick => "Kick",
                    HardMute => "HardMute",
                    Softban => "Softban",
                    Unban => "Unban",
                    _ => "kekw i hate the compiler"
                };
            }
            public DateTime Time { get; set; }
            public static DateTime SetDTime(string DTaime)
            {
                return DateTime.Parse(DTaime);
            }
            public string Reason { get; set; }
            public Infraction() { }
        }
        public class Invite
        {
            public ulong GuildID { get; set; }
            public ulong CreatorID { get; set; }
            public DateTime Created { get; set; }
            public short JoinedNum { get; set; }
            public string Code { get; set; }
            public Invite() { }
        }
        /*public static SqliteCommand Connect()
        {
            
        }*/
        public static string fph = "Data Source = ../Data/rolex.db";
        /// <summary>
        /// Creates a Reader (Query) function skeleton
        /// </summary>
        /// <typeparam name="T">The return SQLite Dtype</typeparam>
        /// <param name="cmdtext">Command text to execute in SQLite</param>
        /// <param name="defval">Default value if no rows are found</param>
        /// <returns>The query reply (if exists) or default value</returns>
        public static async Task<T> QueryFunctionCreator<T>(string cmdtext, T defval)
        {
            using var con = new SqliteConnection(fph);
            await con.OpenAsync();
            using var cmd = new SqliteCommand
            {
                Connection = con,
                CommandText = cmdtext
            };
            var read = await cmd.ExecuteReaderAsync();
            await read.ReadAsync();
            T retval;
            if (!read.HasRows || await read.IsDBNullAsync(0)) retval = defval;
            else {
                retval = (T)read[0]; 
            }
            await read.CloseAsync();
            await con.CloseAsync();
            return retval;
        }
        /// <summary>
        /// Creates a MULTI LINE Reader (Query) function skeleton
        /// </summary>
        /// <typeparam name="T">The return SQLite Dtype</typeparam>
        /// <param name="cmdtext">Command text to execute in SQLite</param>
        /// <param name="defval">Default value if no rows are found</param>
        /// <returns>The query reply (if exists) or default value</returns>
        public static async Task<List<Infraction>> GetInfractions (string cmdtext)
        {
            List<Infraction> retvals = new List<Infraction>();
            using var con = new SqliteConnection(fph);
            await con.OpenAsync();
            using var cmd = new SqliteCommand
            {
                Connection = con,
                CommandText = cmdtext
            };
            var read = await cmd.ExecuteReaderAsync();
            await read.ReadAsync();
            if (!read.HasRows || await read.IsDBNullAsync(0)) return new List<Infraction>();
            else
            {
                do
                {
                    retvals.Add(new Infraction
                    {
                        GuildID = Convert.ToUInt64(read.GetInt64(0)),
                        UserID = Convert.ToUInt64(read.GetInt64(1)),
                        ModeratorID = Convert.ToUInt64(read.GetInt64(2)),
                        Punishment = Infraction.SetPunishment(read.GetString(3)),
                        Time = Infraction.SetDTime(read.GetString(4)),
                        Reason = read.GetString(5)
                    });
                } while (await read.ReadAsync());
            }
            await read.CloseAsync();
            await con.CloseAsync();
            return retvals;
        }
        /// <summary>
        /// Creates a skeleton NonQuery function
        /// </summary>
        /// <param name="cmdtext">The command text to execute in SQLite</param>
        /// <returns></returns>
        public static async Task NonQueryFunctionCreator(string cmdtext)
        {
            using var con = new SqliteConnection(fph);
            await con.OpenAsync();
            using var cmd = new SqliteCommand
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
        public static async Task<string> PrefixGetter(ulong GuilID) => await QueryFunctionCreator($"select Prefix from prefixes where guildid = {GuilID}", "r");
        public static async Task<string> AppealGetter(ulong GuilID) => await QueryFunctionCreator($"select appeal from prefixes where guildid = {GuilID}", "");
        public static async Task<int> AltTimePeriodGetter(ulong GuildID) => await QueryFunctionCreator($"select AltTimeMonths from prefixes where guildid = {GuildID}", 3);
        public static async Task<ulong> MutedRoleIDGetter(ulong GuildID) { 
            var ii = await QueryFunctionCreator($"select MutedRoleID from prefixes where guildid = {GuildID}", long.Parse("0"));
            return Convert.ToUInt64(ii);
        }
        public static async Task<ulong> AlertChanGetter(ulong GuildID)
        {
            var ii = await QueryFunctionCreator($"select AlertChanID from prefixes where GuildID = {GuildID}", long.Parse("0"));
            return Convert.ToUInt64(ii);
        }
        // All adders
        public static async Task MutedRoleIDAdder(ulong GuildID, ulong MutedRoleID) => await NonQueryFunctionCreator($"update prefixes set MutedRoleID = {MutedRoleID} where GuildID = {GuildID};");
        public static async Task PrefixAdder(ulong GuLDID, string prefix)
        {
            await NonQueryFunctionCreator($"update prefixes set Prefix = \"{prefix}\" where GuildID = {GuLDID};");
        }
        public static async Task AppealAdder(ulong GuLDID, string appeallink) => await NonQueryFunctionCreator($"update prefixes set appeal = \"{appeallink}\" where GuildID = {GuLDID};");
        public static async Task AddToModlogs(ulong GuildID, ulong UserID, ulong ModeratorID, Punishment punishment, DateTime time, string Reason = "") {
            await NonQueryFunctionCreator($"insert into modlogs (UserID,GuildID,Punishment,ModeratorID,Time{(Reason == "" ? "" : ",Reason")}) values ({UserID},{GuildID},\"{Enum.GetName(typeof(Punishment), punishment)}\",{ModeratorID},\"{time:o}\"{(Reason == "" ? "" : $",\"{Reason}\"")});");
        }
        public static async Task<List<Infraction>> GetUserModlogs(ulong GuildID, ulong UserID) => await GetInfractions($"select * from modlogs where GuildID = {GuildID} and UserID = {UserID};");
        public static async Task AlertChanAdder(ulong GuildID, ulong ChanID) => await NonQueryFunctionCreator($"update prefixes set AlertChanID = {ChanID} where GuildID = {GuildID};");
        public static async Task AltTimePeriodAdder(ulong GuildID, int AltTimeMonths) => await NonQueryFunctionCreator($"update prefixes set AltTimeMonths = {AltTimeMonths} where GuildID = {GuildID};");
    }
}
