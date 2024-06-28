using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using MovieOpinions.Domain.Entity;
using MovieOpinions.Domain.Entity.Actors;
using MovieOpinions.Domain.Entity.Comments;
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
        private readonly IActorService _actorService;

        public FilmPageController(IGenreService genreService, IFilmsServices filmsService, IUserService userService, ICommentService commentService, IAnswerService answerService, IActorService actorService)
        {
            _genreService = genreService;
            _filmsService = filmsService;
            _userService = userService;
            _commentService = commentService;
            _answerService = answerService;
            _actorService = actorService;
        }

        public async Task<IActionResult> FilmPage()
        {
            FilmPageModel model = new FilmPageModel();
            var GenreAll = await _genreService.GetAllGenre();
            var AllFilms = await _filmsService.GetFilms();

            if(GenreAll.StatusCode == Domain.Enum.StatusCode.OK)
            {
                model.GenreMovies = GenreAll.Data;
            }
            else
            {
                model.GenreMovies = null;
            }

            if (AllFilms.StatusCode == Domain.Enum.StatusCode.OK)
            {
                
                model.Films = AllFilms.Data;
            }
            else
            {
                model.Films = null;
            }

            model.YearsMovies = new List<string> { "1960-1979", "1980-1999", "2000-2019", "2020", "2021", "2022", "2023", "2024" };            
            
            return View(model);
        }

        public async Task<IActionResult> DetailsFilm(int Id)
        {
            var DetailsFilm = await _filmsService.GetFilmId(Id);
            var CommentFilm = await _commentService.GetAllCommentFilm(Id);

            Film film = null;

            if (DetailsFilm.Data != null)
            {
                film = new Film()
                {
                    IdFilm = DetailsFilm.Data.IdFilm,
                    NameFilm = DetailsFilm.Data.NameFilm,
                    YearFilm = DetailsFilm.Data.YearFilm,
                    DescriptionFilm = DetailsFilm.Data.DescriptionFilm,
                    ActorFilm = DetailsFilm.Data.ActorFilm,
                    GenreFilm = DetailsFilm.Data.GenreFilm,
                    CountryFilm = DetailsFilm.Data.CountryFilm,
                    RatingFilm = DetailsFilm.Data.RatingFilm,
                    FilmImage = DetailsFilm.Data.FilmImage,
                    CommentFilm = CommentFilm.Data
                };

                if(film.CommentFilm != null)
                {
                    foreach (var comment in film.CommentFilm)
                    {
                        var UserResponse = await _userService.GetUserId(comment.IdUserComment);
                        var AnswerResponse = await _answerService.GetAnswerToComment(comment.IdComment);

                        if (AnswerResponse.StatusCode == Domain.Enum.StatusCode.OK)
                        {
                            foreach (var answer in AnswerResponse.Data)
                            {
                                comment.AnswerComment.Add(answer);

                                var UserAnswer = await _userService.GetUserId(answer.IdUserAnswer);

                                if (AnswerResponse.StatusCode == Domain.Enum.StatusCode.OK)
                                {
                                    answer.NameUserAnswer = UserAnswer.Data.NameUser;
                                }
                                else
                                {
                                    answer.NameUserAnswer = UserAnswer.Description;
                                }
                            }
                        }

                        if (UserResponse.StatusCode == Domain.Enum.StatusCode.OK)
                        {
                            comment.UserName = UserResponse.Data.NameUser;
                        }
                        else
                        {
                            comment.UserName = UserResponse.Description;
                        }
                    }
                }
                return View(film);
            }
            else
            {
                return View("_ErrorPage");
            }
        }

        public async Task<IActionResult> DetailsActor(int id)
        {
            var GetActor = await _actorService.GetActorById(id);

            if(GetActor.Data != null)
            {
                return View(GetActor.Data);
            }
            else
            {
                return View("_ErrorPage");
            }

        }

        [HttpPost]
        public async Task<IActionResult> GetSortedMoviesGenre([FromBody] List<string> selectedGenres)
        {
            var GetIdGenre = await _genreService.GetIdGenre(selectedGenres);

            if(GetIdGenre.StatusCode == Domain.Enum.StatusCode.OK)
            {
                var GetFilmByGenre = await _filmsService.GetFilmByGenre(GetIdGenre.Data);

                if(GetFilmByGenre.StatusCode == Domain.Enum.StatusCode.OK)
                {
                    return Json(GetFilmByGenre.Data);
                }
                else
                {
                    if(GetFilmByGenre.StatusCode == Domain.Enum.StatusCode.NotFound)
                    {
                        return Json(new { error = "Фільмів не знайдено" });
                    }

                    return Json(new { error = "Помилка сервера. Спробуйте пізніше." + " " + GetFilmByGenre.StatusCode });
                }
            }
            else
            {
                return Json(new { error = "Помилка сервера. Спробуйте пізніше." + " " + GetIdGenre.StatusCode });
            }
        }

        [HttpPost]
        public async Task<IActionResult> GetSortedMoviesYear([FromBody] List<string> selectedYear)
        {
            var GetSelectedFilm = await _filmsService.GetFilmByYear(selectedYear);

            if(GetSelectedFilm.StatusCode == Domain.Enum.StatusCode.OK)
            {
                return Json(GetSelectedFilm.Data);
            }
            else
            {
                return Json(new { error =  GetSelectedFilm.Description });
            }
        }

        [HttpGet]
        public async Task<IActionResult> AllMovies()
        {
            var AllMoviesResponse = await _filmsService.GetFilms();

            if (AllMoviesResponse.StatusCode == Domain.Enum.StatusCode.OK)
            {
                return Json(AllMoviesResponse.Data);
            }
            else
            {
                return Json(new { error = "Помилка сервера. Спробуйте пізніше." + " " + AllMoviesResponse.StatusCode });
            }
        }

        [HttpGet]
        public IActionResult GetAnswerTemplate()
        {
            return PartialView("_AnswerTemplate");
        }

        [HttpGet]
        public IActionResult GetCommentTemplate()
        {
            return PartialView("_CommentTemplate");
        }

        [HttpPost]
        public async Task<IActionResult> AddAnswerToComment([FromBody] Answer DataAnswer)
        {
            var IdUserResult = await _userService.GetUser(DataAnswer.NameUserAnswer);
            
            DataAnswer.IdUserAnswer = IdUserResult.Data.IdUser;
            
            var ResulAddAnswer = await _answerService.AddAnswerDataBase(DataAnswer);
            
            if(ResulAddAnswer.StatusCode == Domain.Enum.StatusCode.OK)
            {
                var CommentFilm = await _commentService.GetIdComment(DataAnswer.IdComment);
            
                if(CommentFilm.StatusCode == Domain.Enum.StatusCode.OK)
                {
                    int IdFilm = CommentFilm.Data.IdFilm;
                    return Json(new { redirectUrl = Url.Action("DetailsFilm", new { id = IdFilm }) });
                }
            }
            return Json(new { description = "Виникла помилка, будь-ласка спробуйте пізніше!" + " " + ResulAddAnswer.StatusCode });
        }

        //public async Task<IActionResult> OpenPrivateOffices()
        //{
        //    
        //}

        public async Task<IActionResult> AddCommentToFilm([FromBody] Comment DataComment, string NameFilm)
        {
            var IdUserResult = await _userService.GetUser(DataComment.UserName);
            
            DataComment.IdUserComment = IdUserResult.Data.IdUser;

            var IdFilm = await _filmsService.GetFilmName(NameFilm);

            DataComment.IdFilm = IdFilm.Data.IdFilm;

            var AddCommentDataBase = await _commentService.AddCommentDataBase(DataComment);

            if(AddCommentDataBase.StatusCode == Domain.Enum.StatusCode.OK)
            {
                return Json(new { redirectUrl = Url.Action("DetailsFilm", new { id = IdFilm.Data.IdFilm }) });
            }
            else
            {
                return Json(new { description = "Виникла помилка, будь-ласка спробуйте пізніше!" + " " + AddCommentDataBase.StatusCode });
            }
        }

        [HttpGet]
        public IActionResult GetRedactionCommentForm()
        {
            return PartialView("_RedactionComment");
        }

        [HttpGet]
        public IActionResult GetRedactionAnswerForm()
        {
            return PartialView("_RedactionAnswer");
        }

        public async Task<IActionResult> ChangeComment([FromBody] Comment DataComment)
        {
            var UpdateComment = await _commentService.EditComment(DataComment);

            if(UpdateComment.StatusCode == Domain.Enum.StatusCode.OK)
            {
                return Json(new { redirectUrl = Url.Action("DetailsFilm", new { id = UpdateComment.Data.IdFilm }) });
            }
            else
            {
                return Json(new { description = "Виникла помилка, будь-ласка спробуйте пізніше!" });
            }
        }

        public async Task<IActionResult> ChangeAnswer([FromBody] Answer DataAnswer)
        {
            var UpdateAnswer = await _answerService.EditAnswer(DataAnswer);

            if(UpdateAnswer.StatusCode != Domain.Enum.StatusCode.OK)
            {
                return Json(new { description = "Виникла помилка, будь-ласка спробуйте пізніше!" });
            }

            var CommentIdResult = await _commentService.GetIdComment(UpdateAnswer.Data.IdComment);

            if(CommentIdResult.StatusCode != Domain.Enum.StatusCode.OK)
            {
                return Json(new { description = "Виникла помилка, будь-ласка спробуйте пізніше!" });
            }

            return Json(new { redirectUrl = Url.Action("DetailsFilm", new { id = CommentIdResult.Data.IdFilm }) });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteComment([FromBody] Comment DataComment)
        {
            var DeleteComment = await _commentService.DeleteComment(DataComment);

            if(DeleteComment.StatusCode != Domain.Enum.StatusCode.OK)
            {
                return Json(new { description = "Виникла помилка, будь-ласка спробуйте пізніше!" });
            }

            return Json(new { redirectUrl = Url.Action("DetailsFilm", new { id = DeleteComment.Data.IdFilm }) });
        }

        [HttpPost]
        public async Task<IActionResult> SearchInformation([FromBody] string DataSearch)
        {
            string Text = DataSearch;
            return Json(new { description = "Виникла помилка, будь-ласка спробуйте пізніше!" });
        }
    }
}