using MovieOpinions.Domain.Entity.Comments;
using MovieOpinions.Domain.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MovieOpinions.DAL.Interface
{
    public interface IAnswerRepository : IBaseRepository<Answer>
    {
        Task<Answer> GetAnswerId(int id);
        Task<IEnumerable<Answer>> GetAnswerUser(int idUser);
        Task<BaseResponse<IEnumerable<Answer>>> GetAnswerComment(int idComment);
    }
}
