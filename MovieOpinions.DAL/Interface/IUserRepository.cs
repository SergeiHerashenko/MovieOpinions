using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MovieOpinions.Domain.Entity;

namespace MovieOpinions.DAL.Interface
{
    public interface IUserRepository : IBaseRepository<User>
    {
        Task<bool> BlockUser(User user);
        Task<User> GetUser(string LoginUser);
    }
}
