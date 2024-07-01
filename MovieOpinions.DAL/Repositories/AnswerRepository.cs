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

namespace MovieOpinions.DAL.Repositories
{
    public class AnswerRepository : IAnswerRepository
    {
        public async Task<BaseResponse<bool>> Create(Answer Entity)
        {
            ConnectMovieOpinions connect = new ConnectMovieOpinions();

            try
            {
                using (var conn = new NpgsqlConnection(connect.ConnectMovieOpinionsDataBase()))
                {
                    await conn.OpenAsync();
                    using (var AddAnser = new NpgsqlCommand("INSERT INTO Answer_Table (id_comment, text_answer, id_user) VALUES (@ID_COMMENT, @TEXT_ANSWER, @ID_USER)", conn))
                    {
                        AddAnser.Parameters.AddWithValue("@ID_COMMENT", Entity.IdComment);
                        AddAnser.Parameters.AddWithValue("@TEXT_ANSWER", Entity.TextAnswer);
                        AddAnser.Parameters.AddWithValue("@ID_USER", Entity.IdUserAnswer);

                        await AddAnser.ExecuteNonQueryAsync();

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

        public async Task<BaseResponse<bool>> Delete(Answer Entity)
        {
            ConnectMovieOpinions connect = new ConnectMovieOpinions();

            using (var conn = new NpgsqlConnection(connect.ConnectMovieOpinionsDataBase()))
            {
                try
                {
                    await conn.OpenAsync();

                    using (var DeleteAnswer = new NpgsqlCommand(
                        "DELETE FROM " +
                            "Answer_Table " +
                        "WHERE " +
                            "id_answer = @ID_ANSWER;", conn))
                    {
                        DeleteAnswer.Parameters.AddWithValue("@ID_ANSWER", Entity.IdAnswer);

                        await DeleteAnswer.ExecuteNonQueryAsync();
                    }

                    return new BaseResponse<bool>()
                    {
                        StatusCode = Domain.Enum.StatusCode.OK,
                        Data = true
                    };
                }
                catch (Exception ex)
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

        public async Task<BaseResponse<IEnumerable<Answer>>> GetAnswerComment(int IdComment)
        {
            List<Answer> answers = new List<Answer>();
            ConnectMovieOpinions connect = new ConnectMovieOpinions();

            using (var conn = new NpgsqlConnection(connect.ConnectMovieOpinionsDataBase()))
            {
                try
                {
                    await conn.OpenAsync();
                    using (var GetAnswer = new NpgsqlCommand(
                        "SELECT id_answer, id_comment, text_answer, id_user " +
                        "FROM Answer_Table " +
                        "WHERE id_comment = @id_comment", conn))
                    {
                        GetAnswer.Parameters.AddWithValue("@id_comment", IdComment);

                        using (var Reader = await GetAnswer.ExecuteReaderAsync())
                        {
                            while (await Reader.ReadAsync())
                            {
                                Answer answer = new Answer
                                {
                                    IdAnswer = Convert.ToInt32(Reader["id_answer"]),
                                    IdComment = Convert.ToInt32(Reader["id_comment"]),
                                    TextAnswer = Reader["text_answer"].ToString(),
                                    IdUserAnswer = Convert.ToInt32(Reader["id_user"])
                                };

                                answers.Add(answer);
                            }
                        }
                    }

                    return new BaseResponse<IEnumerable<Answer>>
                    {
                        StatusCode = Domain.Enum.StatusCode.OK,
                        Data = answers
                    };
                }
                catch (Exception ex)
                {
                    return new BaseResponse<IEnumerable<Answer>>
                    {
                        StatusCode = Domain.Enum.StatusCode.InternalServerError,
                        Description = ex.Message,
                        Data = null
                    };
                }
            }
        }

        public async Task<BaseResponse<Answer>> GetAnswerId(int Id)
        {
            ConnectMovieOpinions connect = new ConnectMovieOpinions();
            Answer answer = null;
            using (var conn = new NpgsqlConnection(connect.ConnectMovieOpinionsDataBase()))
            {
                try
                {
                    await conn.OpenAsync();
                    using (var GetIdAnswer = new NpgsqlCommand(
                        "SELECT id_answer, id_comment, text_answer, id_user " +
                        "FROM Answer_Table " +
                        "WHERE id_answer = @ID_ANSWER", conn))
                    {
                        GetIdAnswer.Parameters.AddWithValue("@ID_ANSWER", Id);

                        using (var Reader = await GetIdAnswer.ExecuteReaderAsync())
                        {
                            if (await Reader.ReadAsync())
                            {
                                answer = new Answer
                                {
                                    IdAnswer = Convert.ToInt32(Reader["id_answer"]),
                                    IdComment = Convert.ToInt32(Reader["id_comment"]),
                                    TextAnswer = Reader["text_answer"].ToString(),
                                    IdUserAnswer = Convert.ToInt32(Reader["id_user"])
                                };
                            }
                        }
                    }
                    return new BaseResponse<Answer>()
                    {
                        StatusCode = Domain.Enum.StatusCode.OK,
                        Data = answer
                    };
                }
                catch (Exception ex)
                {
                    return new BaseResponse<Answer>()
                    {
                        StatusCode = Domain.Enum.StatusCode.NotFound,
                        Description = ex.Message
                    };
                }
            }
        }

        public async Task<BaseResponse<IEnumerable<Answer>>> GetAnswerUser(int IdUser)
        {
            throw new NotImplementedException();
        }

        public async Task<BaseResponse<Answer>> SaveDeleteAnswer(Answer Entity)
        {
            ConnectMovieOpinions connect = new ConnectMovieOpinions();

            try
            {
                using (var conn = new NpgsqlConnection(connect.ConnectMovieOpinionsDataBase()))
                {
                    await conn.OpenAsync();
                    using (var SaveAnswer = new NpgsqlCommand(
                        "INSERT INTO " +
                            "Delete_Answer (id_answer, id_comment, text_answer, id_user) " +
                        "VALUES " +
                            "(@ID_ANSWER, @ID_COMMENT, @TEXT_ANSWER, @ID_USER);", conn))
                    {
                        SaveAnswer.Parameters.AddWithValue("@ID_ANSWER", Entity.IdAnswer);
                        SaveAnswer.Parameters.AddWithValue("@ID_COMMENT", Entity.IdComment);
                        SaveAnswer.Parameters.AddWithValue("@TEXT_ANSWER", Entity.TextAnswer);
                        SaveAnswer.Parameters.AddWithValue("@ID_USER", Entity.IdUserAnswer);

                        await SaveAnswer.ExecuteNonQueryAsync();

                        return new BaseResponse<Answer>
                        {
                            StatusCode = Domain.Enum.StatusCode.OK,
                            Data = Entity
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                return new BaseResponse<Answer>
                {
                    StatusCode = Domain.Enum.StatusCode.InternalServerError,
                    Description = ex.Message
                };
            }
        }

        public async Task<BaseResponse<Answer>> Update(Answer Entity)
        {
            ConnectMovieOpinions connect = new ConnectMovieOpinions();

            using (var conn = new NpgsqlConnection(connect.ConnectMovieOpinionsDataBase()))
            {
                try
                {
                    await conn.OpenAsync();
                    using (var UpdateAnswer = new NpgsqlCommand(
                        "UPDATE " +
                            "Answer_Table " +
                        "SET " +
                            "text_answer = @TEXT_ANSWER " +
                        "WHERE " +
                            "id_answer = @ID_ANSWER", conn))
                    {
                        UpdateAnswer.Parameters.AddWithValue("@TEXT_ANSWER", Entity.TextAnswer);
                        UpdateAnswer.Parameters.AddWithValue("@ID_ANSWER", Entity.IdAnswer);

                        await UpdateAnswer.ExecuteNonQueryAsync();
                    }

                    using(var GetAnswer = new NpgsqlCommand(
                        "SELECT " +
                            "id_answer, id_comment, text_answer, id_user " +
                        "FROM " +
                            "Answer_Table " +
                        "WHERE " +
                            "id_answer = @ID_ANSWER", conn))
                    {
                        GetAnswer.Parameters.AddWithValue("@ID_ANSWER", Entity.IdAnswer);

                        using (var Reader = await GetAnswer.ExecuteReaderAsync())
                        {
                            await Reader.ReadAsync();
                            var UpdatedAnswer = new Answer
                            {
                                IdAnswer = Entity.IdAnswer,
                                IdComment = Convert.ToInt32(Reader["id_comment"]),
                                TextAnswer = Reader["text_answer"].ToString(),
                                IdUserAnswer = Convert.ToInt32(Reader["id_user"])
                            };

                            return new BaseResponse<Answer>
                            {
                                Data = UpdatedAnswer,
                                StatusCode = Domain.Enum.StatusCode.OK
                            };
                        }
                    }
                }
                catch(Exception ex)
                {
                    return new BaseResponse<Answer>
                    {
                        StatusCode = Domain.Enum.StatusCode.InternalServerError,
                        Description = ex.Message
                    };
                }
            }
        }
    }
}
