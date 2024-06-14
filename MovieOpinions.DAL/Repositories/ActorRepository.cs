using MovieOpinions.DAL.Connect_Database;
using MovieOpinions.DAL.Interface;
using MovieOpinions.Domain.Entity;
using MovieOpinions.Domain.Entity.Actors;
using MovieOpinions.Domain.Response;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MovieOpinions.DAL.Repositories
{
    public class ActorRepository : IActorRepository
    {
        public async Task<BaseResponse<bool>> Create(Actor Entity)
        {
            throw new NotImplementedException();
        }

        public async Task<BaseResponse<bool>> Delete(Actor Entity)
        {
            throw new NotImplementedException();
        }

        public async Task<BaseResponse<DetailedActor>> GetActorId(int IdActor)
        {
            DetailedActor actor = null;
            ConnectMovieOpinions connect = new ConnectMovieOpinions();

            using (var conn = new NpgsqlConnection(connect.ConnectMovieOpinionsDataBase()))
            {
                await conn.OpenAsync();
                try
                {
                    using(var getActorId = new NpgsqlCommand(
                        "SELECT " +
                            "Actor_Table.id_actor, " +
                            "Actor_Table.first_name_actor, " +
                            "Actor_Table.last_name_actor, " +
                            "Actor_Table.birthday_actor, " +
                            "Country_Table.name_country, " +
                            "string_agg(DISTINCT Genre_Table.name_genre, ', ') AS GenreActor, " +
                            "string_agg(DISTINCT film_table.name_film, ', ') AS FilmActor " +
                        "FROM " +
                            "Actor_Table " +
                        "LEFT JOIN " +
                            "Actor_Country ON Actor_Table.id_actor = Actor_Country.id_actor " +
                        "LEFT JOIN " +
                            "Country_Table ON Actor_Country.id_country = Country_Table.id_country " +
                        "LEFT JOIN " +
                            "Actor_Genre ON Actor_Table.id_actor = Actor_Genre.id_actor " +
                        "LEFT JOIN " +
                            "Genre_Table ON Actor_Genre.id_genre = Genre_Table.id_genre " +
                        "LEFT JOIN " +
                            "Film_Actor ON Actor_Table.id_actor = Film_Actor.id_actor " +
                        "LEFT JOIN " +
                            "Film_Table ON Film_Actor.id_film = Film_Table.id_film " +
                        "WHERE " +
                            "Actor_Table.id_actor = @idActor " +
                        "GROUP BY " +
                            "Actor_Table.id_actor, " +
                            "Actor_Table.first_name_actor, " +
                            "Actor_Table.last_name_actor, " +
                            "Actor_Table.birthday_actor, " +
                            "Country_Table.name_country;"
                        , conn))
                    {
                        getActorId.Parameters.AddWithValue("@idActor", IdActor);

                        using(var Reader = await getActorId.ExecuteReaderAsync())
                        {
                            if (Reader.Read())
                            {
                                actor = new DetailedActor()
                                {
                                    IdActor = Convert.ToInt32(Reader["id_actor"]),
                                    FirstName = Reader["first_name_actor"].ToString(),
                                    LastName = Reader["last_name_actor"].ToString(),
                                    BirthdayActor = Convert.ToDateTime(Reader["birthday_actor"]),
                                    CountryActor = Reader["name_country"].ToString()
                                };

                                string[] GenreActor = Reader["GenreActor"].ToString().Split(", ");
                                actor.GenreActor = GenreActor;

                                string[] Words = new string[] { actor.FirstName, actor.LastName };
                                string ActorImage = $"/Content/Image_Actor/{string.Join("_", Words)}.jpg";
                                actor.ActorImage = ActorImage;

                                var filmActor = Reader["FilmActor"].ToString().Split(new[] { ", " }, StringSplitOptions.RemoveEmptyEntries);
                                List<Film> filmsList = new List<Film>();
                                foreach(var Film in filmActor)
                                {
                                    var FilmsName = await new FilmRepository().GetMovieName(Film);

                                    Film films = new Film()
                                    {
                                        IdFilm = FilmsName.Data.IdFilm,
                                        NameFilm = FilmsName.Data.NameFilm,
                                        FilmImage = FilmsName.Data.FilmImage
                                    };
                                    filmsList.Add(films);
                                }
                                actor.FilmActor = filmsList;
                            }
                        }
                    }

                    return new BaseResponse<DetailedActor>() 
                    { 
                        StatusCode = Domain.Enum.StatusCode.OK,
                        Data = actor
                    };
                }
                catch (Exception ex)
                {
                    return new BaseResponse<DetailedActor>()
                    {
                        StatusCode = Domain.Enum.StatusCode.InternalServerError,
                        Description = ex.Message
                    };
                }
            }
        }

        public async Task<BaseResponse<Actor>> GetActorName(string NameActor)
        {
            string[] Parts = NameActor.Split(' ');
            Actor actor = new Actor();

            ConnectMovieOpinions connect = new ConnectMovieOpinions();

            using (var conn = new NpgsqlConnection(connect.ConnectMovieOpinionsDataBase()))
            {
                await conn.OpenAsync();
                try
                {
                    using (var GetActor = new NpgsqlCommand("SELECT id_actor, first_name_actor, last_name_actor FROM Actor_Table WHERE first_name_actor = @FirstName AND last_name_actor = @LastName", conn))
                    {
                        GetActor.Parameters.AddWithValue("FirstName", Parts[0]);
                        GetActor.Parameters.AddWithValue("LastName", Parts[1]);

                        using (var Reader = await GetActor.ExecuteReaderAsync())
                        {
                            while (await Reader.ReadAsync())
                            {
                                int ActorId = Convert.ToInt32(Reader["id_actor"]);

                                actor.FirstName = Parts[0];
                                actor.LastName = Parts[1];
                                actor.IdActor = ActorId;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    return new BaseResponse<Actor>()
                    {
                        StatusCode = Domain.Enum.StatusCode.InternalServerError,
                        Description = ex.Message
                    };
                }
            }

            return new BaseResponse<Actor>() 
            { 
                StatusCode = Domain.Enum.StatusCode.OK,
                Data = actor
            };
        }

        public async Task<BaseResponse<Actor>> Update(Actor Entity)
        {
            throw new NotImplementedException();
        }
    }
}
