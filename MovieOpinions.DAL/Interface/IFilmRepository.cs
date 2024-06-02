using MovieOpinions.Domain.Entity;
using MovieOpinions.Domain.Response;
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
        Task<BaseResponse<List<Films>>> GetAll();
        Task<Films> GetMovieName(string Name);
        Task<Films> GetMovieYear(int year);
        Task<Films> GetMovieGenre(string Genre);
    }
}
