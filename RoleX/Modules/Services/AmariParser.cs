using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoleX.Modules.Services
{
    public class AmariWeeklyParser
    {
        public string status { get; set; }
        public List<AmariWeeklyUser> data { get; set; }
        public AmariWeeklyParser() { }
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
        public AmariWeeklyUser() { }
    }
}
