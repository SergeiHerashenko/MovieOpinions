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
            var AddAnswer = await _answerRepository.Create(answer);

            if (AddAnswer.StatusCode != Domain.Enum.StatusCode.OK)
            {
                return new BaseResponse<bool>() 
                {   
                    StatusCode = Domain.Enum.StatusCode.InternalServerError,
                    Description = AddAnswer.Description
                };
            }
            return new BaseResponse<bool>()
            {
                StatusCode = Domain.Enum.StatusCode.OK,
                Data = AddAnswer.Data
            };
        }

        public async Task<BaseResponse<Answer>> EditAnswer(Answer Entity)
        {
            var UpdateAnswer = _answerRepository.Update();
        }

        public async Task<BaseResponse<IEnumerable<Answer>>> GetAnswerToComment(int IdComment)
        {
            var GetAnswer = await _answerRepository.GetAnswerComment(IdComment);

            if(GetAnswer.Data != null)
            {
                return new BaseResponse<IEnumerable<Answer>>()
                {
                    StatusCode = Domain.Enum.StatusCode.OK,
                    Data = GetAnswer.Data
                };
            }
            else
            {
                if(GetAnswer.StatusCode == Domain.Enum.StatusCode.OK)
                {
                    return new BaseResponse<IEnumerable<Answer>>()
                    {
                        StatusCode = Domain.Enum.StatusCode.NotFound,
                        Description = "Відповідей не знайдено"
                    };
                }
                else
                {
                    return new BaseResponse<IEnumerable<Answer>>()
                    {
                        StatusCode = GetAnswer.StatusCode,
                        Description = GetAnswer.Description
                    };
                }
            }
        }
    }
}
