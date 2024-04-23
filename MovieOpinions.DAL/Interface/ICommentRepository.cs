using MovieOpinions.Domain.Entity.Comments;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MovieOpinions.DAL.Interface
{
    public interface ICommentRepository : IBaseRepository<Comment>
    {
        Task<Comment> GetCommentId(int id);
        Task<IEnumerable<Comment>> GetCommentsUser(int idUser);
    }
}
