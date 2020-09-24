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
        public static string fph = "Data Source = rolex.db";
        public static async Task<string> PrefixGetter(ulong GuilID)
        {
            using var con = new SqliteConnection(fph);
            await con.OpenAsync();
            using var cmd = new SqliteCommand();
            cmd.Connection = con;
            cmd.CommandText = $"select Prefix from prefixes where guildid = {GuilID}";
            var read = await cmd.ExecuteReaderAsync();
            await read.ReadAsync();
            if (!read.HasRows) return "role-";
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
            cmd.CommandText = $"replace into prefixes (guildid,Prefix) values ({GuLDID},\"{prefix}\");";
            await cmd.ExecuteNonQueryAsync();
            await con.CloseAsync();
            return;
        }
    }
}
