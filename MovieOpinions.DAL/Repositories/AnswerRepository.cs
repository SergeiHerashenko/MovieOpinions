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
            throw new NotImplementedException();
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
            throw new NotImplementedException();
        }

        public async Task<BaseResponse<IEnumerable<Answer>>> GetAnswerUser(int IdUser)
        {
            throw new NotImplementedException();
        }

        public async Task<BaseResponse<Answer>> Update(Answer Entity)
        {
            throw new NotImplementedException();
        }
    }
}
