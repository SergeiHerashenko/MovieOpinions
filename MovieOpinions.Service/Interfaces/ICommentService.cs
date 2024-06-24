using MovieOpinions.Domain.Entity.Comments;
using MovieOpinions.Domain.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MovieOpinions.Service.Interfaces
{
    public interface ICommentService
    {
        Task<BaseResponse<List<Comment>>> GetAllCommentFilm(int IdFilm);
        Task<BaseResponse<Comment>> GetIdComment(int IdComment);
        Task<BaseResponse<bool>> AddCommentDataBase(Comment comment);
        Task<BaseResponse<Comment>> EditComment(Comment comment);
    }
}
