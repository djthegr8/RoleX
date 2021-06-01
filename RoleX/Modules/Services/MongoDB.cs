using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;
using static RoleX.Modules.Services.SqliteClass;
namespace RoleX.Modules.Services
{
    /*
    [System.AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = true)]
    sealed class M : Attribute
    {
        // See the attribute guidelines at 
        //  http://go.microsoft.com/fwlink/?LinkId=85236
        readonly string positionalString;

        // This is a positional argument
        public MyAttribute(string positionalString)
        {
            this.positionalString = positionalString;

            // TODO: Implement code here

            throw new NotImplementedException();
        }

        public string PositionalString
        {
            get { return positionalString; }
        }

        // This is a named argument
        public int NamedInt { get; set; }
    } */
    public class MongoDB
    {
        public static MongoClient Client = new("yeah this link doesnt work");
        public static async Task RemindersToMongo()
        {
            var rems = await GetReminders("SELECT * from reminders");
            var userCollection = Client.GetDatabase("Guilds").GetCollection<BsonDocument>("User");
            foreach(var UserID in rems.ToHashSet().Select(k => k.UserId))
            {
                var srem = rems.Where(k => k.UserId == UserID);
                var uwlr = new User
                {
                    ID = UserID,
                    Reminders = srem.ToList()                    
                };
                userCollection.InsertOne(uwlr.ToBsonDocument());
            }
        }
        /*public static async Task AddReminder (Reminder rmdr)
        {
            var reminders = Client.GetDatabase("")
        }*/
        public static BsonArray ToBsonDocumentArray<T>(IEnumerable<T> list)
        {
            var array = new BsonArray();
            foreach (var item in list)
            {
                array.Add(item.ToBson());
            }
            return array;
        }
    }
}
