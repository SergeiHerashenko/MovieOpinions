using Microsoft.AspNetCore.Mvc.Rendering;
using MovieOpinions.Domain.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MovieOpinions.Domain.ViewModels.FilmPageModel
{
    public class FilmPageModel
    {
        public string Search { get; set; }
        public List<Film> Films { get; set; }

        public string SelectedSortOption { get; set; }
        public List<SelectListItem> SortOption { get; set; } = new List<SelectListItem>();

        public IEnumerable<string> GenreMovies { get; set; }
        public IEnumerable<string> YearsMovies { get; set; }

        public FilmPageModel()
        {
            SortOption.Add(new SelectListItem { Value = "Alphabetical", Text = "за алфавітом" });
            SortOption.Add(new SelectListItem { Value = "Year", Text = "за роком" });
            SortOption.Add(new SelectListItem { Value = "Popularity", Text = "за популярністю" });
        }
    }
}
