using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using MovieOpinions.Domain.ViewModels.FilmPageModel;
using MovieOpinions.Service.Implementations;
using MovieOpinions.Service.Interfaces;

namespace MovieOpinions.Controllers
{
    public class FilmPageController : Controller
    {
        private readonly IGenreService _genreService;
        private readonly IFilmsServices _filmsService;

        public FilmPageController(IGenreService genreService, IFilmsServices filmsServices)
        {
            _genreService = genreService;
            _filmsService = filmsServices;
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
    }
}
