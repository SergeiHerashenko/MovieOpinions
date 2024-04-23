using MovieOpinions.Domain.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MovieOpinions.DAL.Interface
{
    public interface IFilmRepository : IBaseRepository<Film>
    {
        Task<Film> GetMovieId(int id);
        Task<IEnumerable<Film>> GetAll();
        Task<Film> GetMovieName(string Name);
        Task<Film> GetMovieYear(int year);
        Task<Film> GetMovieGenre(string Genre);
    }
}
