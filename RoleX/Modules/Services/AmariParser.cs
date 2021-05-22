using System.Collections.Generic;

namespace RoleX.Modules.Services
{
    public class AmariWeeklyParser
    {
        public string status { get; set; }
        public List<AmariWeeklyUser> data { get; set; }
        public string message { get; set; }
    }

    public interface IAmariUser
    {
        public string userID { get; set; }
        public string username { get; set; }
        public int uLevel { get; set; }
    }


    public class AmariWeeklyUser : IAmariUser
    {
        public string userID { get; set; }
        public string username { get; set; }
        public int uLevel { get; set; }
        public string weeklyPoints { get; set; }
    }
}
