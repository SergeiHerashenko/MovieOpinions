using MovieOpinions.Domain.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MovieOpinions.DAL.Interface
{
    public interface IFilmRepository : IBaseRepository<Films>
    {
        Task<Films> GetMovieId(int id);
        Task<List<Films>> GetAll();
        Task<Films> GetMovieName(string Name);
        Task<Films> GetMovieYear(int year);
        Task<Films> GetMovieGenre(string Genre);
    }
}
