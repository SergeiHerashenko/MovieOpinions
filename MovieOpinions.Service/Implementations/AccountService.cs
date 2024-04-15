using MovieOpinions.DAL.Interface;
using MovieOpinions.Domain.Entity;
using MovieOpinions.Domain.Helpers;
using MovieOpinions.Domain.Response;
using MovieOpinions.Domain.ViewModels.LoginModel;
using MovieOpinions.Domain.ViewModels.RegisterModel;
using MovieOpinions.Service.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace MovieOpinions.Service.Implementations
{
    public class AccountService : IAccountService
    {
        private readonly IUserRepository _userRepository;

        public AccountService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<BaseResponse<ClaimsIdentity>> Login(LoginModel loginModel)
        {
            try
            {
                var user = await _userRepository.GetUser(loginModel.LoginUser);
                
                if(user != null)
                {
                    bool isPasswordCorrect = await new CheckingCorrectnessPassword().VerifyPassword(loginModel.PasswordUser, user.PasswordSalt, user.PasswordUser);

                    if (isPasswordCorrect)
                    {
                        var result = Authenticate(user);

                        return new BaseResponse<ClaimsIdentity>()
                        {
                            Data = result,
                            StatusCode = Domain.Enum.StatusCode.OK
                        };
                    }
                }

                return new BaseResponse<ClaimsIdentity>()
                {
                    Description = "Невірний логін або пароль!"
                };
            }
            catch (Exception ex)
            {
                return new BaseResponse<ClaimsIdentity>()
                {
                    Description = ex.Message,
                    StatusCode = Domain.Enum.StatusCode.InternalServerError
                };
            }
        }

        public Task<BaseResponse<ClaimsIdentity>> Register(RegisterModel registerModel)
        {
            throw new NotImplementedException();
        }

        private ClaimsIdentity Authenticate(User user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimsIdentity.DefaultNameClaimType, user.NameUser)
            };
            return new ClaimsIdentity(claims, "ApplicationCookie",
                ClaimsIdentity.DefaultNameClaimType, ClaimsIdentity.DefaultRoleClaimType);
        }
    }
}
