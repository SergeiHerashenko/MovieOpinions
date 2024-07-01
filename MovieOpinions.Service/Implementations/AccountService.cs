using Microsoft.AspNetCore.Mvc;
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

        public async Task<BaseResponse<ClaimsIdentity>> Login(LoginModel LoginModel)
        {
            try
            {
                var GetUser = await _userRepository.GetUser(LoginModel.LoginUser);
                
                if(GetUser.StatusCode == Domain.Enum.StatusCode.OK)
                {
                    bool IsPasswordCorrect = await new CheckingCorrectnessPassword().VerifyPassword(LoginModel.PasswordUser, GetUser.Data.PasswordSalt, GetUser.Data.PasswordUser);

                    if (IsPasswordCorrect)
                    {
                        if (GetUser.Data.BlockedUser != true)
                        {
                            var Result = Authenticate(GetUser.Data);

                            return new BaseResponse<ClaimsIdentity>()
                            {
                                Data = Result,
                                StatusCode = Domain.Enum.StatusCode.OK
                            };
                        }
                        else
                        {
                            return new BaseResponse<ClaimsIdentity>()
                            {
                                Description = "Користувач заблокований!",
                                StatusCode = Domain.Enum.StatusCode.BlockedUser
                            };
                        }
                    }
                }
                else
                {
                    if(GetUser.StatusCode == Domain.Enum.StatusCode.NotFound)
                    {
                        return new BaseResponse<ClaimsIdentity>()
                        {
                            StatusCode = Domain.Enum.StatusCode.NotFound,
                            Description = "Невірний логін або пароль!"
                        };
                    }
                }

                return new BaseResponse<ClaimsIdentity>()
                {
                    StatusCode = Domain.Enum.StatusCode.InternalServerError,
                    Description = "Сталася помилка серверу, спробуйте пізніше"
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

        public async Task<BaseResponse<ClaimsIdentity>> Register(RegisterModel RegisterModel)
        {
            try
            {
                var GetUser = await _userRepository.GetUser(RegisterModel.Login);
                if(GetUser.Data != null)
                {
                    return new BaseResponse<ClaimsIdentity>()
                    {
                        Description = "Користувач з таким логіном вже зареєстрований!",
                        StatusCode = Domain.Enum.StatusCode.Conflict
                    };
                }

                string PasswordKey = Guid.NewGuid().ToString();
                string EncryptionPassword = await new HashPassword().GetHashedPassword(RegisterModel.Password, PasswordKey);

                var NewUser = new User()
                {
                    NameUser = RegisterModel.Login,
                    PasswordUser = EncryptionPassword,
                    PasswordSalt = PasswordKey,
                    BlockedUser = false,
                    DeleteUser = false,
                };

                var RegisterUser = await _userRepository.Create(NewUser);

                if (RegisterUser.StatusCode == Domain.Enum.StatusCode.OK)
                {
                    var Result = Authenticate(NewUser);

                    return new BaseResponse<ClaimsIdentity>()
                    {
                        Data = Result,
                        Description = "Користувач зареєстрований!",
                        StatusCode = Domain.Enum.StatusCode.OK
                    };
                }
                else
                {
                    return new BaseResponse<ClaimsIdentity>()
                    {
                        Description = "Упс.. Виникла помилка, спробуйте пізніше!",
                        StatusCode = Domain.Enum.StatusCode.InternalServerError
                    };
                }
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
