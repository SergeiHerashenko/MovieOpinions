using MovieOpinions.Domain.Entity;
using MovieOpinions.Domain.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MovieOpinions.Service.Interfaces
{
    public interface IFilmsServices
    {
        Task<BaseResponse<List<Film>>> GetFilms();
        Task<BaseResponse<Film>> GetFilmId(int Id);
        Task<BaseResponse<Film>> GetFilmName(string NameFilm);
    }
}
