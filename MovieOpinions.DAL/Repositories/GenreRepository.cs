using MovieOpinions.DAL.Connect_Database;
using MovieOpinions.DAL.Interface;
using MovieOpinions.Domain.Response;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace MovieOpinions.DAL.Repositories
{
    public class GenreRepository : IGenreRepository
    {
        public async Task<BaseResponse<IEnumerable<string>>> GetGenre()
        {
            List<string> genres = new List<string>();
            ConnectMovieOpinions connect = new ConnectMovieOpinions();
            using (var conn = new NpgsqlConnection(connect.ConnectMovieOpinionsDataBase()))
            {
                try
                {
                    await conn.OpenAsync();
                    using (var command = new NpgsqlCommand("SELECT name_genre FROM Genre_Table", conn))
                    {
                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (reader.Read())
                            {
                                genres.Add(reader.GetString(0));
                            }
                        }
                    }
                }
                catch(Exception ex)
                {
                    return new BaseResponse<IEnumerable<string>>
                    {
                        StatusCode = Domain.Enum.StatusCode.InternalServerError,
                        Description = ex.Message,
                        Data = null
                    };
                }
            }

            return new BaseResponse<IEnumerable<string>>
            {
                StatusCode = Domain.Enum.StatusCode.OK,
                Data = genres
            };
        }
    }
}
