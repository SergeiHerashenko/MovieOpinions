using MovieOpinions.Domain.Entity;
using MovieOpinions.Domain.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MovieOpinions.DAL.Interface
{
    public interface IFilmRepository : IBaseRepository<Film>
    {
        Task<BaseResponse<Film>> GetMovieId(int Id);
        Task<BaseResponse<List<Film>>> GetAll();
        Task<BaseResponse<Film>> GetMovieName(string Name);
        Task<BaseResponse<List<Film>>> GetMovieYear(IEnumerable<string> Year);
        Task<BaseResponse<List<Film>>> GetMovieGenre(IEnumerable<int> Genre);
    }
}
