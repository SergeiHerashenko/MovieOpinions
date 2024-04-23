using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MovieOpinions.Domain.Entity.Actors
{
    public class DetailedActor : Actor
    {
        public DateTime BirthdayActor { get; set; }
        public IEnumerable<string> FilmActor { get; set; }
        public IEnumerable<string> GenreActor { get; set; }
        public string CountryActor { get; set; }
    }
}
