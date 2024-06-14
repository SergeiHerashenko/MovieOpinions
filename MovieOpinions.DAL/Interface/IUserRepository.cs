using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MovieOpinions.Domain.Entity;
using MovieOpinions.Domain.Response;

namespace MovieOpinions.DAL.Interface
{
    public interface IUserRepository : IBaseRepository<User>
    {
        Task<BaseResponse<bool>> BlockUser(User user);
        Task<BaseResponse<User>> GetUser(string LoginUser);
        Task<BaseResponse<User>> GetUserId(int Id);
    }
}
