using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using MovieOpinions.Domain.Response;
using MovieOpinions.Domain.ViewModels.LoginModel;
using MovieOpinions.Domain.ViewModels.RegisterModel;

namespace MovieOpinions.Service.Interfaces
{
    public interface IAccountService
    {
        Task<BaseResponse<ClaimsIdentity>> Login(LoginModel LoginModel);
        Task<BaseResponse<ClaimsIdentity>> Register(RegisterModel RegisterModel);
    }
}
