using MovieOpinions.DAL.Interface;
using MovieOpinions.Domain.Entity;
using MovieOpinions.Domain.Enum;
using MovieOpinions.Domain.Response;
using MovieOpinions.Service.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MovieOpinions.Service.Implementations
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;

        public UserService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<BaseResponse<User>> GetUser(string UserName)
        {
            var GetUser = await _userRepository.GetUser(UserName);

            if(GetUser.Data != null)
            {
                return new BaseResponse<User>
                {
                    StatusCode = StatusCode.OK,
                    Data = GetUser.Data
                };
            }
            else
            {
                if(GetUser.StatusCode == StatusCode.OK)
                {
                    return new BaseResponse<User>
                    {
                        StatusCode = StatusCode.NotFound,
                        Description = "Користувача не знайдено"
                    };
                }
                else
                {
                    return new BaseResponse<User>
                    {
                        StatusCode = StatusCode.InternalServerError,
                        Description = GetUser.Description
                    };
                }
            }
        }

        public async Task<BaseResponse<User>> GetUserId(int userId)
        {
            var GetIdUser = await _userRepository.GetUserId(userId);

            if (GetIdUser.Data != null)
            {
                return new BaseResponse<User>
                {
                    StatusCode = StatusCode.OK,
                    Data = GetIdUser.Data
                };
            }
            else
            {
                if (GetIdUser.StatusCode == StatusCode.OK)
                {
                    return new BaseResponse<User>
                    {
                        StatusCode = StatusCode.NotFound,
                        Description = "Користувача не знайдено"
                    };
                }
                else
                {
                    return new BaseResponse<User>
                    {
                        StatusCode = StatusCode.InternalServerError,
                        Description = GetIdUser.Description
                    };
                }
            }
        }
    }
}
