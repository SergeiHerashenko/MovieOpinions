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
        public Task<bool> Create(Answer entity)
        {
            throw new NotImplementedException();
        }

        public Task<bool> Delete(Answer entity)
        {
            throw new NotImplementedException();
        }

        public async Task<BaseResponse<IEnumerable<Answer>>> GetAnswerComment(int idComment)
        {
            List<Answer> answers = new List<Answer>();
            ConnectMovieOpinions connect = new ConnectMovieOpinions();

            using (var conn = new NpgsqlConnection(connect.ConnectMovieOpinionsDataBase()))
            {
                try
                {
                    await conn.OpenAsync();
                    using (var command = new NpgsqlCommand(
                        "SELECT id_answer, id_comment, text_answer, id_user " +
                        "FROM Answer_Table " +
                        "WHERE id_comment = @id_comment", conn))
                    {
                        command.Parameters.AddWithValue("@id_comment", idComment);

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                Answer answer = new Answer
                                {
                                    IdAnswer = Convert.ToInt32(reader["id_answer"]),
                                    IdComment = Convert.ToInt32(reader["id_comment"]),
                                    TextAnswer = reader["text_answer"].ToString(),
                                    IdUserAnswer = Convert.ToInt32(reader["id_user"])
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

        public Task<Answer> GetAnswerId(int id)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<Answer>> GetAnswerUser(int idUser)
        {
            throw new NotImplementedException();
        }

        public Task<Answer> Update(Answer entity)
        {
            throw new NotImplementedException();
        }
    }
}
