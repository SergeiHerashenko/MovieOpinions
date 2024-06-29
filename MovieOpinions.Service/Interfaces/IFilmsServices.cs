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
        Task<BaseResponse<List<Film>>> GetFilmByGenre(IEnumerable<int> IdGenre);
        Task<BaseResponse<List<Film>>> GetFilmByYear(IEnumerable<string> Year);
        Task<BaseResponse<List<Film>>> SearchByPartialName(string PartialName);
        Task<BaseResponse<List<Film>>> SortingFilm(string SortElement);
    }
}
