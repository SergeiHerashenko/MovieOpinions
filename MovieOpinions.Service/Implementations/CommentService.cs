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
    public class CommentService : ICommentService
    {
        private readonly ICommentRepository _commentRepository;

        public CommentService(ICommentRepository commentRepository)
        {
            _commentRepository = commentRepository;
        }

        public async Task<BaseResponse<List<Comment>>> GetAllCommentFilm(int idFilm)
        {
            var commentsResponse = await _commentRepository.GetCommentFilm(idFilm);

            if(commentsResponse == null)
            {
                return new BaseResponse<List<Comment>> 
                { 
                    Description = "Коментарів немає",
                    StatusCode = Domain.Enum.StatusCode.NotFound
                };
            }
            else
            {
                return new BaseResponse<List<Comment>>
                {
                    StatusCode = Domain.Enum.StatusCode.OK,
                    Data = commentsResponse.Data
                };
            }
        }
    }
}
