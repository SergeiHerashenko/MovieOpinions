using MovieOpinions.Domain.Entity.Comments;
using MovieOpinions.Domain.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MovieOpinions.DAL.Interface
{
    public interface ICommentRepository : IBaseRepository<Comment>
    {
        Task<BaseResponse<List<Comment>>> GetCommentFilm(int IdFilm);
        Task<BaseResponse<Comment>> GetCommentId(int Id);
        Task<BaseResponse<IEnumerable<Comment>>> GetCommentsUser(int IdUser);
    }
}
