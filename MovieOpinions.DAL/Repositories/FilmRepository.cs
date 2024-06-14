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
        public async Task<BaseResponse<bool>> Create(Film Entity)
        {
            throw new NotImplementedException();
        }

        public async Task<BaseResponse<bool>> Delete(Film Entity)
        {
            throw new NotImplementedException();
        }

        public async Task<BaseResponse<List<Film>>> GetAll()
        {
            List<Film> allFilms = new List<Film>();

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
                        using(var Reader = await GetAllFilms.ExecuteReaderAsync())
                        {
                            while (Reader.Read())
                            {

                                 Film film = new Film()
                                {
                                    IdFilm = Convert.ToInt32(Reader["id_film"]),
                                    NameFilm = Reader["name_film"].ToString(),
                                    YearFilm = Convert.ToInt32(Reader["year_film"]),
                                    DescriptionFilm = Reader["description_film"].ToString()
                                };

                                var ActorsName = Reader["actors"].ToString().Split(new[] { ", " }, StringSplitOptions.RemoveEmptyEntries);
                                List<Actor> actors = new List<Actor>();
                                foreach(string NameActor in ActorsName)
                                {
                                    var Actor = await new ActorRepository().GetActorName(NameActor);
                                    Actor actor = new Actor
                                    {
                                        IdActor = Actor.Data.IdActor,
                                        FirstName = Actor.Data.FirstName,
                                        LastName = Actor.Data.LastName,
                                    };
                                    actors.Add(actor);
                                }
                                film.ActorFilm = actors;

                                string[] GenreNames = Reader["genres"].ToString().Split(", ");
                                film.GenreFilm = GenreNames.ToList();

                                string[] CountryNames = Reader["countries"].ToString().Split(", ");
                                film.CountryFilm = CountryNames.ToList();

                                string[] RatingStrings = Reader["ratings"].ToString().Split(", ");
                                List<int> ratings = new List<int>();

                                foreach (string RatingString in RatingStrings)
                                {
                                    if (int.TryParse(RatingString, out int rating))
                                    {
                                        ratings.Add(rating);
                                    }
                                }
                                film.RatingFilm = ratings != null && ratings.Any() ? ratings.Sum() / (double)ratings.Count : 0;

                                string[] Words = Regex.Split(film.NameFilm, @"\W+");
                                string FilmImage = $"/Content/Image_Films/{string.Join("_", Words)}.jpg";
                                film.FilmImage = FilmImage;

                                allFilms.Add(film);
                            }
                        }
                    }
                }
                catch(Exception ex)
                {
                    return new BaseResponse<List<Film>>
                    {
                        StatusCode = Domain.Enum.StatusCode.InternalServerError,
                        Description = ex.Message
                    };
                }
            }

            return new BaseResponse<List<Film>>
            {
                StatusCode = Domain.Enum.StatusCode.OK,
                Data = allFilms
            };
        }

        public async Task<BaseResponse<Film>> GetMovieGenre(string Genre)
        {
            throw new NotImplementedException();
        }

        public async Task<BaseResponse<Film>> GetMovieId(int Id)
        {
            Film film = null;
            ConnectMovieOpinions connect = new ConnectMovieOpinions();

            using (var conn = new NpgsqlConnection(connect.ConnectMovieOpinionsDataBase()))
            {
                try
                {
                    await conn.OpenAsync();

                    using (var GetFilmId = new NpgsqlCommand(
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
                            "Film_Table.id_film =  @id_film " +
                        "GROUP BY " +
                            "Film_Table.id_film, " +
                            "Film_Table.name_film, " +
                            "Film_Table.year_film, " +
                            "Film_Table.description_film"
                        , conn))
                    {
                        GetFilmId.Parameters.AddWithValue("@id_film", Id);

                        using (var Reader = await GetFilmId.ExecuteReaderAsync())
                        {
                            while (Reader.Read())
                            {

                                film = new Film()
                                {
                                    IdFilm = Convert.ToInt32(Reader["id_film"]),
                                    NameFilm = Reader["name_film"].ToString(),
                                    YearFilm = Convert.ToInt32(Reader["year_film"]),
                                    DescriptionFilm = Reader["description_film"].ToString()
                                };

                                var ActorsName = Reader["actors"].ToString().Split(new[] { ", " }, StringSplitOptions.RemoveEmptyEntries);
                                List<Actor> actors = new List<Actor>();
                                foreach (string NameActor in ActorsName)
                                {
                                    var Actor = await new ActorRepository().GetActorName(NameActor);
                                    Actor actor = new Actor
                                    {
                                        IdActor = Actor.Data.IdActor,
                                        FirstName = Actor.Data.FirstName,
                                        LastName = Actor.Data.LastName,
                                    };
                                    actors.Add(actor);
                                }
                                film.ActorFilm = actors;

                                string[] GenreNames = Reader["genres"].ToString().Split(", ");
                                film.GenreFilm = GenreNames.ToList();

                                string[] CountryNames = Reader["countries"].ToString().Split(", ");
                                film.CountryFilm = CountryNames.ToList();

                                string[] RatingStrings = Reader["ratings"].ToString().Split(", ");
                                List<int> ratings = new List<int>();

                                foreach (string RatingString in RatingStrings)
                                {
                                    if (int.TryParse(RatingString, out int rating))
                                    {
                                        ratings.Add(rating);
                                    }
                                }
                                film.RatingFilm = ratings != null && ratings.Any() ? ratings.Sum() / (double)ratings.Count : 0;

                                string[] Words = Regex.Split(film.NameFilm, @"\W+");
                                string FilmImage = $"/Content/Image_Films/{string.Join("_", Words)}.jpg";
                                film.FilmImage = FilmImage;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    return new BaseResponse<Film>
                    {
                        StatusCode = Domain.Enum.StatusCode.InternalServerError,
                        Description = ex.Message
                    };
                }

                return new BaseResponse<Film>
                {
                    StatusCode = Domain.Enum.StatusCode.OK,
                    Data = film
                };
            }
        }

        public async Task<BaseResponse<Film>> GetMovieName(string Name)
        {
            Film film = null;
            ConnectMovieOpinions connect = new ConnectMovieOpinions();

            using (var conn = new NpgsqlConnection(connect.ConnectMovieOpinionsDataBase()))
            {
                try
                {
                    await conn.OpenAsync();

                    using (var GetFilmName = new NpgsqlCommand(
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
                            "Film_Table.name_film =  @name_film " +
                        "GROUP BY " +
                            "Film_Table.id_film, " +
                            "Film_Table.name_film, " +
                            "Film_Table.year_film, " +
                            "Film_Table.description_film"
                        , conn))
                    {
                        GetFilmName.Parameters.AddWithValue("@name_film", Name);

                        using (var Reader = await GetFilmName.ExecuteReaderAsync())
                        {
                            while (Reader.Read())
                            {

                                film = new Film()
                                {
                                    IdFilm = Convert.ToInt32(Reader["id_film"]),
                                    NameFilm = Reader["name_film"].ToString(),
                                    YearFilm = Convert.ToInt32(Reader["year_film"]),
                                    DescriptionFilm = Reader["description_film"].ToString()
                                };

                                var ActorsName = Reader["actors"].ToString().Split(new[] { ", " }, StringSplitOptions.RemoveEmptyEntries);
                                List<Actor> actors = new List<Actor>();
                                foreach (string NameActor in ActorsName)
                                {
                                    var Actor = await new ActorRepository().GetActorName(NameActor);
                                    Actor actor = new Actor
                                    {
                                        IdActor = Actor.Data.IdActor,
                                        FirstName = Actor.Data.FirstName,
                                        LastName = Actor.Data.LastName,
                                    };
                                    actors.Add(actor);
                                }
                                film.ActorFilm = actors;

                                string[] GenreNames = Reader["genres"].ToString().Split(", ");
                                film.GenreFilm = GenreNames.ToList();

                                string[] CountryNames = Reader["countries"].ToString().Split(", ");
                                film.CountryFilm = CountryNames.ToList();

                                string[] RatingStrings = Reader["ratings"].ToString().Split(", ");
                                List<int> ratings = new List<int>();

                                foreach (string RatingString in RatingStrings)
                                {
                                    if (int.TryParse(RatingString, out int rating))
                                    {
                                        ratings.Add(rating);
                                    }
                                }
                                film.RatingFilm = ratings != null && ratings.Any() ? ratings.Sum() / (double)ratings.Count : 0;

                                string[] Words = Regex.Split(film.NameFilm, @"\W+");
                                string FilmImage = $"/Content/Image_Films/{string.Join("_", Words)}.jpg";
                                film.FilmImage = FilmImage;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    return new BaseResponse<Film>
                    {
                        StatusCode = Domain.Enum.StatusCode.InternalServerError,
                        Description = ex.Message
                    };
                }

                return new BaseResponse<Film>
                {
                    StatusCode = Domain.Enum.StatusCode.OK,
                    Data = film
                };
            }
        }

        public async Task<BaseResponse<Film>> GetMovieYear(int Year)
        {
            throw new NotImplementedException();
        }

        public async Task<BaseResponse<Film>> Update(Film Entity)
        {
            throw new NotImplementedException();
        }
    }
}
