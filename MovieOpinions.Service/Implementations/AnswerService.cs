using MovieOpinions.DAL.Interface;
using MovieOpinions.Domain.Entity.Comments;
using MovieOpinions.Domain.Response;
using MovieOpinions.Service.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MovieOpinions.Service.Implementations
{
    public class AnswerService : IAnswerService
    {
        private readonly IAnswerRepository _answerRepository;

        public AnswerService(IAnswerRepository answerRepository)
        {
            _answerRepository = answerRepository;
        }

        public async Task<BaseResponse<bool>> AddAnswerDataBase(Answer answer)
        {
            return await _answerRepository.Create(answer);
        }

        public async Task<BaseResponse<IEnumerable<Answer>>> GetAnswerToComment(int idComment)
        {
            return await _answerRepository.GetAnswerComment(idComment);
        }
    }
}
