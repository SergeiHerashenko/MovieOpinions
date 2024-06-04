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

        public async Task<BaseResponse<User>> GetUserId(int userId)
        {
            var getUser = await _userRepository.GetUserId(userId);

            if (getUser == null)
            {
                return CreateResponse(StatusCode.NotFound, "Unknown");
            }

            if (getUser.BlockedUser)
            {
                return CreateResponse(StatusCode.BlockedUser, "Заблокований користувач");
            }

            if (getUser.DeleteUser)
            {
                return CreateResponse(StatusCode.DeleteUser, "Видалений користувач");
            }

            return CreateResponse(StatusCode.OK, data: getUser);
        }

        private BaseResponse<User> CreateResponse(StatusCode statusCode, string description = null, User data = null)
        {
            return new BaseResponse<User>
            {
                StatusCode = statusCode,
                Description = description,
                Data = data
            };
        }
    }
}
