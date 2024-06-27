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
                    var SortedComment = CommentsResponse.Data.OrderBy(c => c.IdComment).ToList();

                    return new BaseResponse<List<Comment>>
                    {
                        StatusCode = Domain.Enum.StatusCode.OK,
                        Data = SortedComment
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

        public async Task<BaseResponse<Comment>> EditComment(Comment comment)
        {
            var CommentResponse = await _commentRepository.Update(comment);

            if(CommentResponse.StatusCode == Domain.Enum.StatusCode.OK)
            {
                return new BaseResponse<Comment>()
                {
                    StatusCode = Domain.Enum.StatusCode.OK,
                    Data = CommentResponse.Data
                };
            }
            else
            {
                return new BaseResponse<Comment>()
                {
                    StatusCode = Domain.Enum.StatusCode.InternalServerError,
                    Data = null,
                    Description = CommentResponse.Description
                };
            }
        }

        public async Task<BaseResponse<Comment>> DeleteComment(Comment Entity)
        {
            var GetComment = await _commentRepository.GetCommentId(Entity.IdComment);

            if(GetComment.Data != null && GetComment.StatusCode == Domain.Enum.StatusCode.OK)
            {
                var SaveComment = await _commentRepository.SaveDeleteComment(GetComment.Data);

                if(SaveComment.StatusCode == Domain.Enum.StatusCode.OK)
                {
                    var DeleteComment = await _commentRepository.Delete(SaveComment.Data);

                    return new BaseResponse<Comment>()
                    {
                        Data= SaveComment.Data,
                        StatusCode = Domain.Enum.StatusCode.OK
                    };
                }
                else
                {
                    return new BaseResponse<Comment>()
                    {
                        StatusCode = Domain.Enum.StatusCode.InternalServerError,
                        Description = SaveComment.Description
                    };
                }
            }
            else
            {
                return new BaseResponse<Comment>()
                {
                    StatusCode = Domain.Enum.StatusCode.NotFound,
                    Description = "Коментар не знайдено"
                };
            }
        }
    }
}
