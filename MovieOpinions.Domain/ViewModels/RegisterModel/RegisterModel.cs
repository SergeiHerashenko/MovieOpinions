using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace MovieOpinions.Domain.ViewModels.RegisterModel
{
    public class RegisterModel
    {
        [Display(Name = "Login")]
        public string Login { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "PasswordUser")]
        public string PasswordConfirm { get; set; }
    }
}
