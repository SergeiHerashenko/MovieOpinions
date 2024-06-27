using MovieOpinions.DAL.Connect_Database;
using MovieOpinions.DAL.Interface;
using MovieOpinions.Domain.Entity;
using MovieOpinions.Domain.Entity.Comments;
using MovieOpinions.Domain.Response;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using System.Xml.Linq;

namespace MovieOpinions.DAL.Repositories
{
    public class CommentRepository : ICommentRepository
    {
        public async Task<BaseResponse<bool>> Create(Comment Entity)
        {
            ConnectMovieOpinions connect = new ConnectMovieOpinions();

            try
            {
                using (var conn = new NpgsqlConnection(connect.ConnectMovieOpinionsDataBase()))
                {
                    await conn.OpenAsync();
                    using (var AddComment = new NpgsqlCommand("INSERT INTO Comment_Table (id_user, text_comment, id_film, date_comment) VALUES (@ID_USER, @TEXT_COMMENT, @ID_FILM, now())", conn))
                    {
                        AddComment.Parameters.AddWithValue("@ID_USER", Entity.IdUserComment);
                        AddComment.Parameters.AddWithValue("@TEXT_COMMENT", Entity.TextComment);
                        AddComment.Parameters.AddWithValue("@ID_FILM", Entity.IdFilm);

                        await AddComment.ExecuteNonQueryAsync();

                        return new BaseResponse<bool>
                        {
                            StatusCode = Domain.Enum.StatusCode.OK,
                            Data = true
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                return new BaseResponse<bool>
                {
                    StatusCode = Domain.Enum.StatusCode.InternalServerError,
                    Data = false,
                    Description = ex.Message
                };
            }
        }

        public async Task<BaseResponse<bool>> Delete(Comment Entity)
        {
            ConnectMovieOpinions connect = new ConnectMovieOpinions();

            using (var conn = new NpgsqlConnection(connect.ConnectMovieOpinionsDataBase()))
            {
                try
                {
                    await conn.OpenAsync();

                    using (var DeleteComment = new NpgsqlCommand(
                        "DELETE FROM " +
                            "Comment_Table " +
                        "WHERE " +
                            "id_comment = @ID_COMMENT;", conn))
                    {
                        DeleteComment.Parameters.AddWithValue("@ID_COMMENT", Entity.IdComment);

                        await DeleteComment.ExecuteNonQueryAsync();
                    }

                    return new BaseResponse<bool>()
                    {
                        StatusCode = Domain.Enum.StatusCode.OK,
                        Data = true
                    };
                }
                catch(Exception ex)
                {
                    return new BaseResponse<bool>()
                    {
                        StatusCode = Domain.Enum.StatusCode.InternalServerError,
                        Description = ex.Message,
                        Data = false
                    };
                }
            }
        }

        public async Task<BaseResponse<List<Comment>>> GetCommentFilm(int IdFilm)
        {
            List<Comment> comments = new List<Comment>();
            ConnectMovieOpinions connect = new ConnectMovieOpinions();

            using (var conn = new NpgsqlConnection(connect.ConnectMovieOpinionsDataBase()))
            {
                try
                {
                    await conn.OpenAsync();
                    using (var GetFilmComment = new NpgsqlCommand(
                        "SELECT id_comment, id_user, text_comment, date_comment, id_film " +
                        "FROM Comment_Table " +
                        "WHERE id_film = @id_film", conn))
                    {
                        GetFilmComment.Parameters.AddWithValue("@id_film", IdFilm);

                        using (var Reader = await GetFilmComment.ExecuteReaderAsync())
                        {
                            while (await Reader.ReadAsync())
                            {
                                Comment comment = new Comment
                                {
                                    IdComment = Convert.ToInt32(Reader["id_comment"]),
                                    IdUserComment = Convert.ToInt32(Reader["id_user"]),
                                    TextComment = Reader["text_comment"].ToString(),
                                    IdFilm = Convert.ToInt32(Reader["id_film"]),
                                    DateComment = Convert.ToDateTime(Reader["date_comment"])
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
                        Description = ex.Message
                    };
                }
            }

            return new BaseResponse<List<Comment>>
            {
                StatusCode = Domain.Enum.StatusCode.OK,
                Data = comments
            };
        }

        public async Task<BaseResponse<Comment>> GetCommentId(int Id)
        {
            ConnectMovieOpinions connect = new ConnectMovieOpinions();
            Comment comment = null;
            using (var conn = new NpgsqlConnection(connect.ConnectMovieOpinionsDataBase()))
            {
                try
                {
                    await conn.OpenAsync();
                    using (var GetIdComment = new NpgsqlCommand(
                        "SELECT id_comment, id_user, text_comment, id_film, date_comment " +
                        "FROM Comment_Table " +
                        "WHERE id_comment = @id_comment", conn))
                    {
                        GetIdComment.Parameters.AddWithValue("@id_comment", Id);

                        using (var Reader = await GetIdComment.ExecuteReaderAsync())
                        {
                            if (await Reader.ReadAsync())
                            {
                                comment = new Comment
                                {
                                    IdComment = Convert.ToInt32(Reader["id_comment"]),
                                    IdUserComment = Convert.ToInt32(Reader["id_user"]),
                                    TextComment = Reader["text_comment"].ToString(),
                                    IdFilm = Convert.ToInt32(Reader["id_film"]),
                                    DateComment = Convert.ToDateTime(Reader["date_comment"])
                                };
                            }
                        }
                    }
                    return new BaseResponse<Comment>()
                    {
                        StatusCode = Domain.Enum.StatusCode.OK,
                        Data = comment
                    };
                }
                catch (Exception ex)
                {
                    return new BaseResponse<Comment>()
                    {
                        StatusCode = Domain.Enum.StatusCode.NotFound,
                        Description = ex.Message
                    };
                }
            }

        }

        public async Task<BaseResponse<IEnumerable<Comment>>> GetCommentsUser(int IdUser)
        {
            throw new NotImplementedException();
        }

        public async Task<BaseResponse<Comment>> SaveDeleteComment(Comment Entity)
        {
            ConnectMovieOpinions connect = new ConnectMovieOpinions();

            try
            {
                using (var conn = new NpgsqlConnection(connect.ConnectMovieOpinionsDataBase()))
                {
                    await conn.OpenAsync();
                    using (var SaveComment = new NpgsqlCommand(
                        "INSERT INTO " +
                            "Delete_Comment (id_comment, id_user, text_comment, id_film, date_comment) " +
                        "VALUES " +
                            "(@ID_COMMENT, @ID_USER, @TEXT_COMMENT, @ID_FILM, @DATE_COMMENT);", conn))
                    {
                        SaveComment.Parameters.AddWithValue("@ID_COMMENT", Entity.IdComment);
                        SaveComment.Parameters.AddWithValue("@ID_USER", Entity.IdUserComment);
                        SaveComment.Parameters.AddWithValue("@TEXT_COMMENT", Entity.TextComment);
                        SaveComment.Parameters.AddWithValue("@ID_FILM", Entity.IdFilm);
                        SaveComment.Parameters.AddWithValue("@DATE_COMMENT", Entity.DateComment);

                        await SaveComment.ExecuteNonQueryAsync();

                        return new BaseResponse<Comment>
                        {
                            StatusCode = Domain.Enum.StatusCode.OK,
                            Data = Entity
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                return new BaseResponse<Comment>
                {
                    StatusCode = Domain.Enum.StatusCode.InternalServerError,
                    Description = ex.Message
                };
            }
        }

        public async Task<BaseResponse<Comment>> Update(Comment Entity)
        {
            ConnectMovieOpinions connect = new ConnectMovieOpinions();
            using (var conn = new NpgsqlConnection(connect.ConnectMovieOpinionsDataBase()))
            {
                try
                {
                    await conn.OpenAsync();
                    using(var UpdateComment = new NpgsqlCommand(
                        "UPDATE " +
                            "Comment_Table " +
                        "SET " +
                            "text_comment = @TEXT_COMMENT " +
                        "WHERE " +
                            "id_comment = @ID_COMMENT", conn))
                    {
                        UpdateComment.Parameters.AddWithValue("@TEXT_COMMENT", Entity.TextComment);
                        UpdateComment.Parameters.AddWithValue("@ID_COMMENT", Entity.IdComment);

                        await UpdateComment.ExecuteNonQueryAsync();
                    }

                    using (var GetComment = new NpgsqlCommand(
                        "SELECT " +
                            "id_comment, id_user, text_comment, id_film, date_comment " +
                        "FROM " +
                            "Comment_Table " +
                        "WHERE " +
                            "id_comment = @ID_COMMENT", conn))
                    {
                        GetComment.Parameters.AddWithValue("@ID_COMMENT", Entity.IdComment);

                        using(var Reader = await GetComment.ExecuteReaderAsync())
                        {
                            await Reader.ReadAsync();
                            var UpdatedComment = new Comment
                            {
                                IdComment = Entity.IdComment,
                                IdUserComment = Convert.ToInt32(Reader["id_user"]),
                                TextComment = Reader["text_comment"].ToString(),
                                IdFilm = Convert.ToInt32(Reader["id_film"]),
                                DateComment = Convert.ToDateTime(Reader["date_comment"]),
                                AnswerComment = null
                            };

                            return new BaseResponse<Comment>
                            {
                                Data = UpdatedComment,
                                StatusCode = Domain.Enum.StatusCode.OK
                            };
                        }
                    }
                }
                catch (Exception ex)
                {
                    return new BaseResponse<Comment>
                    {
                        Data = null,
                        StatusCode = Domain.Enum.StatusCode.InternalServerError,
                        Description = ex.Message
                    };
                }
            }
        }
    }
}
