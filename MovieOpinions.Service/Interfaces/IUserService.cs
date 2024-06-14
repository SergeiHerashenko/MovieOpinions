using MovieOpinions.Domain.Entity;
using MovieOpinions.Domain.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MovieOpinions.Service.Interfaces
{
    public interface IUserService
    {
        Task<BaseResponse<User>> GetUserId(int UserId);
        Task<BaseResponse<User>> GetUser(string UserName);
    }
}
