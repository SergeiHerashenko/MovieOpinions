using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MovieOpinions.Domain.Entity
{
    public class User
    {
        public int IdUser { get; set; }
        public string NameUser { get; set; }
        public string PasswordUser { get; set; }
        public string PasswordSalt { get; set; }
        public bool DeleteUser { get; set; }
        public bool BlockedUser { get; set; }
    }
}
