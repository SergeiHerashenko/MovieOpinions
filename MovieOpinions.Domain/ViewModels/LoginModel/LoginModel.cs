using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace MovieOpinions.Domain.ViewModels.LoginModel
{
    public class LoginModel
    {
        [Display(Name = "LoginUser")]
        public string LoginUser { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "PasswordUser")]
        public string PasswordUser { get; set; }
    }
}
