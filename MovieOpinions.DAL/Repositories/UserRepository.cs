using MovieOpinions.DAL.Connect_Database;
using MovieOpinions.DAL.Interface;
using MovieOpinions.Domain.Entity;
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
        public Task<bool> BlockUser(User user)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> Create(User entity)
        {
            ConnectMovieOpinions connect = new ConnectMovieOpinions();
            using(var conn = new NpgsqlConnection(connect.ConnectMovieOpinionsDataBase()))
            {
                try
                {
                    await conn.OpenAsync();
                    using (var command = new NpgsqlCommand("INSERT INTO User_Table (user_name, user_password, salt_password, delete_user, blocked_user)" +
                                "VALUES (@n1, @q1, @w1, @p1, @a1)", conn))
                    {
                        command.Parameters.AddWithValue("n1", entity.NameUser);
                        command.Parameters.AddWithValue("q1", entity.PasswordUser);
                        command.Parameters.AddWithValue("w1", entity.PasswordSalt);
                        command.Parameters.AddWithValue("p1", entity.DeleteUser);
                        command.Parameters.AddWithValue("a1", entity.BlockedUser);

                        command.ExecuteNonQuery();
                    }

                    return true;
                }
                catch (Exception ex)
                {
                    return false;
                }
            }
        }

        public Task<bool> Delete(User entity)
        {
            throw new NotImplementedException();
        }

        public async Task<User> GetUser(string LoginUser)
        {
            User user = null;

            ConnectMovieOpinions connectDatabase = new ConnectMovieOpinions();

            using (var conn = new NpgsqlConnection(connectDatabase.ConnectMovieOpinionsDataBase()))
            {
                await conn.OpenAsync();

                using(var getUserCommand = new NpgsqlCommand("SELECT * FROM User_Table WHERE user_name = @LoginUser", conn))
                {
                    getUserCommand.Parameters.AddWithValue("@LoginUser", LoginUser);

                    using(var readerInformationUser = await getUserCommand.ExecuteReaderAsync())
                    {
                        while (readerInformationUser.Read())
                        {
                            user = new User
                            {
                                IdUser = Convert.ToInt32(readerInformationUser["id_user"]),
                                NameUser = readerInformationUser["user_name"].ToString(),
                                PasswordUser = readerInformationUser["user_password"].ToString(),
                                PasswordSalt = readerInformationUser["salt_password"].ToString(),
                                DeleteUser = Convert.ToBoolean(readerInformationUser["delete_user"]),
                                BlockedUser = Convert.ToBoolean(readerInformationUser["blocked_user"])
                            };
                        }
                    }
                }
            }

            return user;
        }

        public async Task<User> GetUserId(int id)
        {
            ConnectMovieOpinions connectDatabase = new ConnectMovieOpinions();

            User user = null;

            using (var conn = new NpgsqlConnection(connectDatabase.ConnectMovieOpinionsDataBase()))
            {
                await conn.OpenAsync();

                using (var getUserIdCommand = new NpgsqlCommand("SELECT id_user, user_name, user_password, salt_password, delete_user, blocked_user FROM User_Table WHERE id_user = @idUser", conn))
                {
                    getUserIdCommand.Parameters.AddWithValue("idUser", id);

                    using (var readerInformationUser = await getUserIdCommand.ExecuteReaderAsync())
                    {
                        while (readerInformationUser.Read())
                        {
                            user = new User
                            {
                                IdUser = Convert.ToInt32(readerInformationUser["id_user"]),
                                NameUser = readerInformationUser["user_name"].ToString(),
                                PasswordUser = readerInformationUser["user_password"].ToString(),
                                PasswordSalt = readerInformationUser["salt_password"].ToString(),
                                DeleteUser = Convert.ToBoolean(readerInformationUser["delete_user"]),
                                BlockedUser = Convert.ToBoolean(readerInformationUser["blocked_user"])
                            };
                        }
                    }
                }
            }
            return user;
        }

        public Task<User> Update(User entity)
        {
            throw new NotImplementedException();
        }
    }
}
