using MovieOpinions.DAL.Connect_Database;
using MovieOpinions.DAL.Interface;
using MovieOpinions.Domain.Entity;
using MovieOpinions.Domain.Response;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MovieOpinions.DAL.Repositories
{
    public class UserRepository : IUserRepository
    {
        public async Task<BaseResponse<bool>> BlockUser(User user)
        {
            throw new NotImplementedException();
        }

        public async Task<BaseResponse<bool>> Create(User Entity)
        {
            ConnectMovieOpinions connect = new ConnectMovieOpinions();
            using(var conn = new NpgsqlConnection(connect.ConnectMovieOpinionsDataBase()))
            {
                try
                {
                    await conn.OpenAsync();
                    using (var CreateUser = new NpgsqlCommand("INSERT INTO User_Table (user_name, user_password, salt_password, delete_user, blocked_user)" +
                                "VALUES (@n1, @q1, @w1, @p1, @a1)", conn))
                    {
                        CreateUser.Parameters.AddWithValue("n1", Entity.NameUser);
                        CreateUser.Parameters.AddWithValue("q1", Entity.PasswordUser);
                        CreateUser.Parameters.AddWithValue("w1", Entity.PasswordSalt);
                        CreateUser.Parameters.AddWithValue("p1", Entity.DeleteUser);
                        CreateUser.Parameters.AddWithValue("a1", Entity.BlockedUser);

                        CreateUser.ExecuteNonQuery();
                    }

                    return new BaseResponse<bool> 
                    { 
                        StatusCode = Domain.Enum.StatusCode.OK
                    };
                }
                catch (Exception ex)
                {
                    return new BaseResponse<bool>
                    {
                        StatusCode = Domain.Enum.StatusCode.InternalServerError,
                        Description = ex.Message
                    };
                }
            }
        }

        public async Task<BaseResponse<bool>> Delete(User Entity)
        {
            throw new NotImplementedException();
        }

        public async Task<BaseResponse<User>> GetUser(string LoginUser)
        {
            User user = null;

            ConnectMovieOpinions connectDatabase = new ConnectMovieOpinions();

            using (var conn = new NpgsqlConnection(connectDatabase.ConnectMovieOpinionsDataBase()))
            {
                try
                {
                    await conn.OpenAsync();

                    using (var GetUserCommand = new NpgsqlCommand("SELECT * FROM User_Table WHERE user_name = @LoginUser", conn))
                    {
                        GetUserCommand.Parameters.AddWithValue("@LoginUser", LoginUser);

                        using (var ReaderInformationUser = await GetUserCommand.ExecuteReaderAsync())
                        {
                            while (ReaderInformationUser.Read())
                            {
                                user = new User
                                {
                                    IdUser = Convert.ToInt32(ReaderInformationUser["id_user"]),
                                    NameUser = ReaderInformationUser["user_name"].ToString(),
                                    PasswordUser = ReaderInformationUser["user_password"].ToString(),
                                    PasswordSalt = ReaderInformationUser["salt_password"].ToString(),
                                    DeleteUser = Convert.ToBoolean(ReaderInformationUser["delete_user"]),
                                    BlockedUser = Convert.ToBoolean(ReaderInformationUser["blocked_user"])
                                };
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    return new BaseResponse<User>()
                    {
                        StatusCode = Domain.Enum.StatusCode.InternalServerError,
                        Description = ex.Message
                    };
                }
                
            }

            return new BaseResponse<User>() 
            {
                StatusCode = Domain.Enum.StatusCode.OK,
                Data = user
            };
        }

        public async Task<BaseResponse<User>> GetUserId(int Id)
        {
            ConnectMovieOpinions connectDatabase = new ConnectMovieOpinions();

            User user = null;

            using (var conn = new NpgsqlConnection(connectDatabase.ConnectMovieOpinionsDataBase()))
            {
                try
                {
                    await conn.OpenAsync();

                    using (var GetUserIdCommand = new NpgsqlCommand("SELECT id_user, user_name, user_password, salt_password, delete_user, blocked_user FROM User_Table WHERE id_user = @idUser", conn))
                    {
                        GetUserIdCommand.Parameters.AddWithValue("idUser", Id);

                        using (var ReaderInformationUser = await GetUserIdCommand.ExecuteReaderAsync())
                        {
                            while (ReaderInformationUser.Read())
                            {
                                user = new User
                                {
                                    IdUser = Convert.ToInt32(ReaderInformationUser["id_user"]),
                                    NameUser = ReaderInformationUser["user_name"].ToString(),
                                    PasswordUser = ReaderInformationUser["user_password"].ToString(),
                                    PasswordSalt = ReaderInformationUser["salt_password"].ToString(),
                                    DeleteUser = Convert.ToBoolean(ReaderInformationUser["delete_user"]),
                                    BlockedUser = Convert.ToBoolean(ReaderInformationUser["blocked_user"])
                                };
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    return new BaseResponse<User>()
                    {
                        StatusCode = Domain.Enum.StatusCode.InternalServerError,
                        Description = ex.Message
                    };
                }
            }
            return new BaseResponse<User>() 
            {
                StatusCode = Domain.Enum.StatusCode.OK,
                Data = user
            };
        }

        public async Task<BaseResponse<User>> Update(User Entity)
        {
            throw new NotImplementedException();
        }
    }
}
