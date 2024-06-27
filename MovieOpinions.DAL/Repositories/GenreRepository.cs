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
                    using (var GetGenreAll = new NpgsqlCommand("SELECT name_genre FROM Genre_Table", conn))
                    {
                        using (var Reader = await GetGenreAll.ExecuteReaderAsync())
                        {
                            while (Reader.Read())
                            {
                                genres.Add(Reader.GetString(0));
                            }
                        }
                    }
                }
                catch(Exception ex)
                {
                    return new BaseResponse<IEnumerable<string>>
                    {
                        StatusCode = Domain.Enum.StatusCode.InternalServerError,
                        Description = ex.Message
                    };
                }
            }

            return new BaseResponse<IEnumerable<string>>
            {
                StatusCode = Domain.Enum.StatusCode.OK,
                Data = genres
            };
        }

        public async Task<BaseResponse<IEnumerable<int>>> GetGenreId(IEnumerable<string> NameGenre)
        {
            List<int> idGenres = new List<int>();

            string NameGenreString = string.Join(", ", NameGenre.Select(genre => $"'{genre}'"));

            ConnectMovieOpinions connect = new ConnectMovieOpinions();

            using(var conn = new NpgsqlConnection(connect.ConnectMovieOpinionsDataBase()))
            {
                try
                {
                    await conn.OpenAsync();

                    using(var GetGenreId = new NpgsqlCommand(
                        "SELECT " +
                            "id_genre " +
                        "FROM " +
                            "Genre_Table " +
                        "WHERE " +
                            $"name_genre IN ({NameGenreString})", conn))
                    {
                        using (var Reader = await GetGenreId.ExecuteReaderAsync())
                        {
                            while (Reader.Read())
                            {
                                idGenres.Add(Convert.ToInt32(Reader["id_genre"]));
                            }
                        }
                    }

                    return new BaseResponse<IEnumerable<int>>()
                    {
                        Data = idGenres,
                        StatusCode = Domain.Enum.StatusCode.OK
                    };
                }
                catch(Exception ex)
                {
                    return new BaseResponse<IEnumerable<int>>()
                    {
                        StatusCode = Domain.Enum.StatusCode.InternalServerError,
                        Description = ex.Message
                    };
                }
            }
        }
    }
}
