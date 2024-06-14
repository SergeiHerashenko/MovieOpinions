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
        Task<BaseResponse<Answer>> GetAnswerId(int Id);
        Task<BaseResponse<IEnumerable<Answer>>> GetAnswerUser(int IdUser);
        Task<BaseResponse<IEnumerable<Answer>>> GetAnswerComment(int IdComment);
    }
}
