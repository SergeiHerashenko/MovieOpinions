using MovieOpinions.Domain.Entity.Actors;
using MovieOpinions.Domain.Entity.Comments;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MovieOpinions.Domain.Entity
{
    public class Films
    {
        public int IdFilm { get; set; }
        public string NameFilm { get; set; }
        public int YearFilm { get; set; }
        public string DescriptionFilm { get; set; }
        public IEnumerable<Actor> ActorFilm { get; set; }
        public IEnumerable<string> GenreFilm { get; set; }
        public IEnumerable<string> CountryFilm { get; set; }
        public double RatingFilm { get; set; }
        public string FilmImage { get; set; }
    }
}
