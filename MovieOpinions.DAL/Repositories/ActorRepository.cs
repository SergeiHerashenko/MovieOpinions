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
        public Task<BaseResponse<bool>> Create(Actor entity)
        {
            throw new NotImplementedException();
        }

        public Task<bool> Delete(Actor entity)
        {
            throw new NotImplementedException();
        }

        public async Task<DetailedActor> GetActorId(int idActor)
        {
            DetailedActor actor = new DetailedActor();
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
                        getActorId.Parameters.AddWithValue("@idActor", idActor);

                        using(var reader = await getActorId.ExecuteReaderAsync())
                        {
                            if (reader.Read())
                            {
                                actor = new DetailedActor()
                                {
                                    IdActor = Convert.ToInt32(reader["id_actor"]),
                                    FirstName = reader["first_name_actor"].ToString(),
                                    LastName = reader["last_name_actor"].ToString(),
                                    BirthdayActor = Convert.ToDateTime(reader["birthday_actor"]),
                                    CountryActor = reader["name_country"].ToString()
                                };

                                string[] genreActor = reader["GenreActor"].ToString().Split(", ");
                                actor.GenreActor = genreActor;

                                string[] words = new string[] { actor.FirstName, actor.LastName };
                                string actorImage = $"/Content/Image_Actor/{string.Join("_", words)}.jpg";
                                actor.ActorImage = actorImage;

                                var filmActor = reader["FilmActor"].ToString().Split(new[] { ", " }, StringSplitOptions.RemoveEmptyEntries);
                                List<Film> filmsList = new List<Film>();
                                foreach(var film in filmActor)
                                {
                                    var FilmsName = await new FilmRepository().GetMovieName(film);

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

                    return actor;
                }
                catch (Exception ex)
                {
                    return actor;
                }
            }
        }

        public async Task<Actor> GetActorName(string NameActor)
        {
            string[] parts = NameActor.Split(' ');
            Actor actor = new Actor();

            ConnectMovieOpinions connect = new ConnectMovieOpinions();

            using (var conn = new NpgsqlConnection(connect.ConnectMovieOpinionsDataBase()))
            {
                await conn.OpenAsync();
                try
                {
                    using (var GetActor = new NpgsqlCommand("SELECT id_actor, first_name_actor, last_name_actor FROM Actor_Table WHERE first_name_actor = @FirstName AND last_name_actor = @LastName", conn))
                    {
                        GetActor.Parameters.AddWithValue("FirstName", parts[0]);
                        GetActor.Parameters.AddWithValue("LastName", parts[1]);

                        using (var reader = await GetActor.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                int actorId = Convert.ToInt32(reader["id_actor"]);

                                actor.FirstName = parts[0];
                                actor.LastName = parts[1];
                                actor.IdActor = actorId;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {

                }
            }

            return actor;
        }

        public Task<Actor> Update(Actor entity)
        {
            throw new NotImplementedException();
        }
    }
}
