using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using MovieOpinions.Domain.Entity;
using MovieOpinions.Domain.ViewModels.FilmPageModel;
using MovieOpinions.Service.Implementations;
using MovieOpinions.Service.Interfaces;
using System.Reflection;

namespace MovieOpinions.Controllers
{
    public class FilmPageController : Controller
    {
        private readonly IGenreService _genreService;
        private readonly IFilmsServices _filmsService;
        private readonly IUserService _userService;
        private readonly ICommentService _commentService;
        private readonly IAnswerService _answerService;

        public FilmPageController(IGenreService genreService, IFilmsServices filmsService, IUserService userService, ICommentService commentService, IAnswerService answerService)
        {
            _genreService = genreService;
            _filmsService = filmsService;
            _userService = userService;
            _commentService = commentService;
            _answerService = answerService;
        }

        public async Task<IActionResult> FilmPage()
        {
            FilmPageModel model = new FilmPageModel();
            var genre = await _genreService.GetAllGenre();
            var films = await _filmsService.GetFilms();

            if(genre.StatusCode == Domain.Enum.StatusCode.OK)
            {
                model.GenreMovies = genre.Data;
            }
            else
            {
                model.GenreMovies = null;
            }

            if (films.StatusCode == Domain.Enum.StatusCode.OK)
            {
                
                model.Films = films.Data;
            }
            else
            {
                model.Films = null;
            }

            model.YearsMovies = new List<string> { "1960-1979", "1980-1999", "2000-2019", "2020", "2021", "2022", "2023", "2024" };            
            
            return View(model);
        }

        public async Task<IActionResult> DetailsFilm(int id)
        {
            var detailsFilm = await _filmsService.GetFilmId(id);
            var commentFilm = await _commentService.GetAllCommentFilm(id);

            Film film = null;

            if (detailsFilm.Data != null)
            {
                film = new Film()
                {
                    IdFilm = detailsFilm.Data.IdFilm,
                    NameFilm = detailsFilm.Data.NameFilm,
                    YearFilm = detailsFilm.Data.YearFilm,
                    DescriptionFilm = detailsFilm.Data.DescriptionFilm,
                    ActorFilm = detailsFilm.Data.ActorFilm,
                    GenreFilm = detailsFilm.Data.GenreFilm,
                    CountryFilm = detailsFilm.Data.CountryFilm,
                    RatingFilm = detailsFilm.Data.RatingFilm,
                    FilmImage = detailsFilm.Data.FilmImage,
                    CommentFilm = commentFilm.Data
                };

                foreach (var comment in film.CommentFilm)
                {
                    var userResponse = await _userService.GetUserId(comment.IdUserComment);
                    var answerResponse = await _answerService.GetAnswerToComment(comment.IdComment);

                    if(answerResponse.StatusCode == Domain.Enum.StatusCode.OK)
                    {
                        foreach(var answer in answerResponse.Data)
                        {
                            comment.AnswerComment.Add(answer);
                        }
                    }

                    if (userResponse.StatusCode == Domain.Enum.StatusCode.OK)
                    {
                        comment.UserName = userResponse.Data.NameUser; 
                    }
                    else
                    {
                        comment.UserName = userResponse.Description;
                    }
                }
            }
            return View(film);
        }
    }
}
