using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks; 
using Microsoft.Data.Sqlite;
namespace TradeMemer.modules
{
    class SqliteClass
    {
        /*public static SqliteCommand Connect()
        {
            
        }*/
        public static string fph = "Data Source = Data/rolex.db";
        public static async Task<string> PrefixGetter(ulong GuilID)
        {
            using var con = new SqliteConnection(fph);
            await con.OpenAsync();
            using var cmd = new SqliteCommand();
            cmd.Connection = con;
            cmd.CommandText = $"select Prefix from prefixes where guildid = {GuilID}";
            var read = await cmd.ExecuteReaderAsync();
            await read.ReadAsync();
            if (!read.HasRows || await read.IsDBNullAsync(0)) return "role-";
            var pref = read.GetString(0);
            await read.CloseAsync();
            await con.CloseAsync();
            return pref;
        }
        public static async Task<string> AppealGetter(ulong GuilID)
        {
            using var con = new SqliteConnection(fph);
            await con.OpenAsync();
            using var cmd = new SqliteCommand();
            cmd.Connection = con;
            cmd.CommandText = $"select appeal from prefixes where guildid = {GuilID}";
            var read = await cmd.ExecuteReaderAsync();
            await read.ReadAsync();
            if (!read.HasRows) return "";
            var pref = read.GetString(0);
            await read.CloseAsync();
            await con.CloseAsync();
            return pref;
        }
        public static async Task PrefixAdder(ulong GuLDID, string prefix)
        {
            using var con = new SqliteConnection(fph);
            await con.OpenAsync();
            using var cmd = new SqliteCommand();
            cmd.Connection = con;
            cmd.CommandText = $"replace into prefixes (guildid,Prefix,appeal) values ({GuLDID},\"{prefix}\",\"{await AppealGetter(GuLDID)}\");";
            await cmd.ExecuteNonQueryAsync();
            await con.CloseAsync();
            return;
        }
        public static async Task AppealAdder(ulong GuLDID, string appeallink)
        {
            using var con = new SqliteConnection(fph);
            await con.OpenAsync();
            using var cmd = new SqliteCommand();
            cmd.Connection = con;
            cmd.CommandText = $"replace into prefixes (guildid,Prefix,appeal) values ({GuLDID},\"{await PrefixGetter(GuLDID)}\",\"{appeallink}\");";
            await cmd.ExecuteNonQueryAsync();
            await con.CloseAsync();
            return;
        }
    }
}
