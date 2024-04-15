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

        public Task<bool> Create(User entity)
        {
            throw new NotImplementedException();
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

            return user;
        }

        public Task<User> Update(User entity)
        {
            throw new NotImplementedException();
        }
    }
}
