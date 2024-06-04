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
    public class FilmRepository : IFilmRepository
    {
        public Task<bool> Create(Films entity)
        {
            throw new NotImplementedException();
        }

        public Task<bool> Delete(Films entity)
        {
            throw new NotImplementedException();
        }

        public async Task<BaseResponse<List<Films>>> GetAll()
        {
            List<Films> AllFilms = new List<Films>();

            ConnectMovieOpinions connect = new ConnectMovieOpinions();

            using (var conn = new NpgsqlConnection(connect.ConnectMovieOpinionsDataBase()))
            {
                try
                {
                    await conn.OpenAsync();
                    using (var GetAllFilms = new NpgsqlCommand(
                        "SELECT " +
                            "Film_Table.id_film, " +
                            "Film_Table.name_film, " +
                            "Film_Table.year_film, " +
                            "Film_Table.description_film, " +
                            "STRING_AGG(DISTINCT CONCAT(Actor_Table.first_name_actor, ' ', Actor_Table.last_name_actor), ', ') AS actors, " +
                            "STRING_AGG(DISTINCT Genre_Table.name_genre, ', ') AS genres, " +
                            "STRING_AGG(DISTINCT Country_Table.name_country, ', ') AS countries, " +
                            "STRING_AGG(Film_Rating.rating_film::TEXT, ', ') AS ratings " +
                        "FROM " +
                            "Film_Table " +
                        "LEFT JOIN " +
                            "Film_Actor ON Film_Table.id_film = Film_Actor.id_film " +
                        "LEFT JOIN " +
                            "Actor_Table ON Film_Actor.id_actor = Actor_Table.id_actor " +
                        "LEFT JOIN " +
                            "Film_Genre ON Film_Table.id_film = Film_Genre.id_film " +
                        "LEFT JOIN " +
                            "Genre_Table ON Film_Genre.id_genre = Genre_Table.id_genre " +
                        "LEFT JOIN " +
                            "Film_Country ON Film_Table.id_film = Film_Country.id_film " +
                        "LEFT JOIN " +
                            "Country_Table ON Film_Country.id_country = Country_Table.id_country " +
                        "LEFT JOIN " +
                            "Film_Rating ON Film_Table.id_film = Film_Rating.id_film " +
                        "GROUP BY " +
                            "Film_Table.id_film, " +
                            "Film_Table.name_film, " +
                            "Film_Table.year_film, " +
                            "Film_Table.description_film"
                        , conn))
                    {
                        using(var reader = await GetAllFilms.ExecuteReaderAsync())
                        {
                            while (reader.Read())
                            {

                                Films film = new Films()
                                {
                                    IdFilm = Convert.ToInt32(reader["id_film"]),
                                    NameFilm = reader["name_film"].ToString(),
                                    YearFilm = Convert.ToInt32(reader["year_film"]),
                                    DescriptionFilm = reader["description_film"].ToString()
                                };

                                var ActorsName = reader["actors"].ToString().Split(new[] { ", " }, StringSplitOptions.RemoveEmptyEntries);
                                List<Actor> actors = new List<Actor>();
                                foreach(string NameActor in ActorsName)
                                {
                                    var Actor = await new ActorRepository().GetActorName(NameActor);
                                    Actor actor = new Actor
                                    {
                                        IdActor = Actor.IdActor,
                                        FirstName = Actor.FirstName,
                                        LastName = Actor.LastName,
                                    };
                                    actors.Add(actor);
                                }
                                film.ActorFilm = actors;

                                string[] GenreNames = reader["genres"].ToString().Split(", ");
                                film.GenreFilm = GenreNames.ToList();

                                string[] CountryNames = reader["countries"].ToString().Split(", ");
                                film.CountryFilm = CountryNames.ToList();

                                string[] ratingStrings = reader["ratings"].ToString().Split(", ");
                                List<int> ratings = new List<int>();

                                foreach (string ratingString in ratingStrings)
                                {
                                    if (int.TryParse(ratingString, out int rating))
                                    {
                                        ratings.Add(rating);
                                    }
                                }
                                film.RatingFilm = ratings != null && ratings.Any() ? ratings.Sum() / (double)ratings.Count : 0;

                                string[] words = Regex.Split(film.NameFilm, @"\W+");
                                string filmImage = $"/Content/Image_Films/{string.Join("_", words)}.jpg";
                                film.FilmImage = filmImage;

                                AllFilms.Add(film);
                            }
                        }
                    }
                }
                catch(Exception ex)
                {
                    return new BaseResponse<List<Films>>
                    {
                        StatusCode = Domain.Enum.StatusCode.InternalServerError,
                        Description = ex.Message,
                        Data = null
                    };
                }
            }
            return new BaseResponse<List<Films>>
            {
                StatusCode = Domain.Enum.StatusCode.OK,
                Data = AllFilms
            };
        }

        public Task<Films> GetMovieGenre(string Genre)
        {
            throw new NotImplementedException();
        }

        public async Task<BaseResponse<Film>> GetMovieId(int id)
        {
            Film film = null;
            ConnectMovieOpinions connect = new ConnectMovieOpinions();

            using (var conn = new NpgsqlConnection(connect.ConnectMovieOpinionsDataBase()))
            {
                try
                {
                    await conn.OpenAsync();

                    using (var GetFilm = new NpgsqlCommand(
                        "SELECT " +
                            "Film_Table.id_film, " +
                            "Film_Table.name_film, " +
                            "Film_Table.year_film, " +
                            "Film_Table.description_film, " +
                            "STRING_AGG(DISTINCT CONCAT(Actor_Table.first_name_actor, ' ', Actor_Table.last_name_actor), ', ') AS actors, " +
                            "STRING_AGG(DISTINCT Genre_Table.name_genre, ', ') AS genres, " +
                            "STRING_AGG(DISTINCT Country_Table.name_country, ', ') AS countries, " +
                            "STRING_AGG(Film_Rating.rating_film::TEXT, ', ') AS ratings " +
                        "FROM " +
                            "Film_Table " +
                        "LEFT JOIN " +
                            "Film_Actor ON Film_Table.id_film = Film_Actor.id_film " +
                        "LEFT JOIN " +
                            "Actor_Table ON Film_Actor.id_actor = Actor_Table.id_actor " +
                        "LEFT JOIN " +
                            "Film_Genre ON Film_Table.id_film = Film_Genre.id_film " +
                        "LEFT JOIN " +
                            "Genre_Table ON Film_Genre.id_genre = Genre_Table.id_genre " +
                        "LEFT JOIN " +
                            "Film_Country ON Film_Table.id_film = Film_Country.id_film " +
                        "LEFT JOIN " +
                            "Country_Table ON Film_Country.id_country = Country_Table.id_country " +
                        "LEFT JOIN " +
                            "Film_Rating ON Film_Table.id_film = Film_Rating.id_film " +
                        "WHERE " +
                        "film_table.id_film =  @id_film " +
                        "GROUP BY " +
                            "Film_Table.id_film, " +
                            "Film_Table.name_film, " +
                            "Film_Table.year_film, " +
                            "Film_Table.description_film"
                        , conn))
                    {
                        GetFilm.Parameters.AddWithValue("@id_film", id);

                        using (var reader = await GetFilm.ExecuteReaderAsync())
                        {
                            while (reader.Read())
                            {

                                film = new Film()
                                {
                                    IdFilm = Convert.ToInt32(reader["id_film"]),
                                    NameFilm = reader["name_film"].ToString(),
                                    YearFilm = Convert.ToInt32(reader["year_film"]),
                                    DescriptionFilm = reader["description_film"].ToString()
                                };

                                var ActorsName = reader["actors"].ToString().Split(new[] { ", " }, StringSplitOptions.RemoveEmptyEntries);
                                List<Actor> actors = new List<Actor>();
                                foreach (string NameActor in ActorsName)
                                {
                                    var Actor = await new ActorRepository().GetActorName(NameActor);
                                    Actor actor = new Actor
                                    {
                                        IdActor = Actor.IdActor,
                                        FirstName = Actor.FirstName,
                                        LastName = Actor.LastName,
                                    };
                                    actors.Add(actor);
                                }
                                film.ActorFilm = actors;

                                string[] GenreNames = reader["genres"].ToString().Split(", ");
                                film.GenreFilm = GenreNames.ToList();

                                string[] CountryNames = reader["countries"].ToString().Split(", ");
                                film.CountryFilm = CountryNames.ToList();

                                string[] ratingStrings = reader["ratings"].ToString().Split(", ");
                                List<int> ratings = new List<int>();

                                foreach (string ratingString in ratingStrings)
                                {
                                    if (int.TryParse(ratingString, out int rating))
                                    {
                                        ratings.Add(rating);
                                    }
                                }
                                film.RatingFilm = ratings != null && ratings.Any() ? ratings.Sum() / (double)ratings.Count : 0;

                                string[] words = Regex.Split(film.NameFilm, @"\W+");
                                string filmImage = $"/Content/Image_Films/{string.Join("_", words)}.jpg";
                                film.FilmImage = filmImage;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    return new BaseResponse<Film>
                    {
                        StatusCode = Domain.Enum.StatusCode.InternalServerError,
                        Description = ex.Message,
                        Data = null
                    };
                }

                return new BaseResponse<Film>
                {
                    StatusCode = Domain.Enum.StatusCode.OK,
                    Data = film
                };
            }
        }

        public Task<Film> GetMovieName(string Name)
        {
            throw new NotImplementedException();
        }

        public Task<Films> GetMovieYear(int year)
        {
            throw new NotImplementedException();
        }

        public Task<Films> Update(Films entity)
        {
            throw new NotImplementedException();
        }
    }
}
