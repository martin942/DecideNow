using DecideNowServer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DecideNowServer
{
    public class RegisterModel : ModelBase
    {
        public string user_name { get; set; }
        public string first_name { get; set; }
        public string last_name { get; set; }
        public string email { get; set; }
        public string birth_date { get; set; }
        public string password_hash { get; set; }
        public string public_key { get; set; }
        public string check_sum { get; set; }
        public string create_date { get; set; }
    }
}
