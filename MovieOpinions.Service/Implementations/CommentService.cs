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

        public async Task<BaseResponse<bool>> AddCommentDataBase(Comment comment)
        {
            var AddComment = await _commentRepository.Create(comment);

            if (AddComment.StatusCode == Domain.Enum.StatusCode.OK)
            {
                return new BaseResponse<bool>
                {
                    Description = "Коментар додано",
                    StatusCode = Domain.Enum.StatusCode.OK
                };
            }
            else
            {
                return new BaseResponse<bool>
                {
                    Description = "Коментар не додано",
                    StatusCode = Domain.Enum.StatusCode.InternalServerError
                };
            }
        }

        public async Task<BaseResponse<List<Comment>>> GetAllCommentFilm(int IdFilm)
        {
            var CommentsResponse = await _commentRepository.GetCommentFilm(IdFilm);

            if(CommentsResponse.StatusCode != Domain.Enum.StatusCode.OK)
            {
                return new BaseResponse<List<Comment>>
                {
                    Description = CommentsResponse.Description,
                    StatusCode = Domain.Enum.StatusCode.InternalServerError
                };
            }
            else
            {
                if(CommentsResponse.Data.Count != 0)
                {
                    return new BaseResponse<List<Comment>>
                    {
                        StatusCode = Domain.Enum.StatusCode.OK,
                        Data = CommentsResponse.Data
                    };
                }
                else
                {
                    return new BaseResponse<List<Comment>>
                    {
                        StatusCode = Domain.Enum.StatusCode.NotFound,
                        Description = "Коментарів не знайдено"
                    };
                }
            }
        }

        public async Task<BaseResponse<Comment>> GetIdComment(int IdComment)
        {
            var Comment = await _commentRepository.GetCommentId(IdComment);

            if(Comment.Data != null)
            {
                return new BaseResponse<Comment>
                {
                    Data = Comment.Data,
                    StatusCode = Domain.Enum.StatusCode.OK
                };
            }
            else
            {
                if(Comment.StatusCode == Domain.Enum.StatusCode.OK)
                {
                    return new BaseResponse<Comment>
                    {
                        Description = "Коментар не знайдено",
                        StatusCode = Domain.Enum.StatusCode.NotFound
                    };
                }
                else
                {
                    return new BaseResponse<Comment>
                    {
                        Description = Comment.Description,
                        StatusCode = Domain.Enum.StatusCode.InternalServerError
                    };
                }
            }
        }
    }
}
