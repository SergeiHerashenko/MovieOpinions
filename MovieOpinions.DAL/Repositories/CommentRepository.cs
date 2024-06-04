using MovieOpinions.DAL.Connect_Database;
using MovieOpinions.DAL.Interface;
using MovieOpinions.Domain.Entity.Comments;
using MovieOpinions.Domain.Response;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MovieOpinions.DAL.Repositories
{
    public class CommentRepository : ICommentRepository
    {
        public Task<bool> Create(Comment entity)
        {
            throw new NotImplementedException();
        }

        public Task<bool> Delete(Comment entity)
        {
            throw new NotImplementedException();
        }

        public async Task<BaseResponse<List<Comment>>> GetCommentFilm(int idFilm)
        {
            List<Comment> comments = new List<Comment>();
            ConnectMovieOpinions connect = new ConnectMovieOpinions();

            using (var conn = new NpgsqlConnection(connect.ConnectMovieOpinionsDataBase()))
            {
                try
                {
                    await conn.OpenAsync();
                    using (var command = new NpgsqlCommand(
                        "SELECT id_comment, id_user, text_comment, date_comment, id_film " +
                        "FROM comment_table " +
                        "WHERE id_film = @id_film", conn))
                    {
                        command.Parameters.AddWithValue("@id_film", idFilm);

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                Comment comment = new Comment
                                {
                                    IdComment = Convert.ToInt32(reader["id_comment"]),
                                    IdUserComment = Convert.ToInt32(reader["id_user"]),
                                    TextComment = reader["text_comment"].ToString(),
                                    IdFilm = Convert.ToInt32(reader["id_film"]),
                                    DateComment = Convert.ToDateTime(reader["date_comment"])
                                };

                                comments.Add(comment);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    return new BaseResponse<List<Comment>>
                    {
                        StatusCode = Domain.Enum.StatusCode.InternalServerError,
                        Description = ex.Message,
                        Data = null
                    };
                }
            }

            return new BaseResponse<List<Comment>>
            {
                StatusCode = Domain.Enum.StatusCode.OK,
                Data = comments
            };
        }

        public Task<Comment> GetCommentId(int id)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<Comment>> GetCommentsUser(int idUser)
        {
            throw new NotImplementedException();
        }

        public Task<Comment> Update(Comment entity)
        {
            throw new NotImplementedException();
        }
    }
}
