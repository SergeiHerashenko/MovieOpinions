using MovieOpinions.DAL.Interface;
using MovieOpinions.Domain.Entity;
using MovieOpinions.Domain.Response;
using MovieOpinions.Service.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MovieOpinions.Service.Implementations
{
    public class FilmsServices : IFilmsServices
    {
        private readonly IFilmRepository _filmRepository;

        public FilmsServices(IFilmRepository filmRepository)
        {
            _filmRepository = filmRepository;
        }

        public async Task<BaseResponse<List<Film>>> GetFilms()
        {
            return await _filmRepository.GetAll();
        }

        public async Task<BaseResponse<Film>> GetFilmId(int id)
        {
            var filmResponse = await _filmRepository.GetMovieId(id);

            if (filmResponse.StatusCode != Domain.Enum.StatusCode.OK || filmResponse.Data == null)
            {
                return new BaseResponse<Film>
                {
                    StatusCode = filmResponse.StatusCode,
                    Description = filmResponse.Description
                };
            }

            return filmResponse;
        }
    }
}
