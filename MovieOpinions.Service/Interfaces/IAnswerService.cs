using MovieOpinions.Domain.Entity.Comments;
using MovieOpinions.Domain.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MovieOpinions.Service.Interfaces
{
    public interface IAnswerService
    {
        Task<BaseResponse<IEnumerable<Answer>>> GetAnswerToComment(int IdComment);
        Task<BaseResponse<bool>> AddAnswerDataBase(Answer answer);
    }
}
