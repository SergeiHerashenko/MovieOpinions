using MovieOpinions.DAL.Interface;
using MovieOpinions.Domain.Response;
using MovieOpinions.Service.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MovieOpinions.Service.Implementations
{
    public class GenreService : IGenreService
    {
        private readonly IGenreRepository _genreRepository;

        public GenreService(IGenreRepository genreRepository)
        {
            _genreRepository = genreRepository;
        }

        public async Task<BaseResponse<IEnumerable<string>>> GetAllGenre()
        {
            var getGenres = await _genreRepository.GetGenre();

            if (getGenres == null)
            {
                return new BaseResponse<IEnumerable<string>>()
                {
                    Description = "Жанрів не знайдено.",
                    StatusCode = Domain.Enum.StatusCode.NotFound
                };
            }

            return new BaseResponse<IEnumerable<string>>()
            {
                Data = getGenres.StatusCode == Domain.Enum.StatusCode.OK ? getGenres.Data : null,
                Description = getGenres.Description,
                StatusCode = getGenres.StatusCode == Domain.Enum.StatusCode.OK
                    ? Domain.Enum.StatusCode.OK
                    : Domain.Enum.StatusCode.InternalServerError
            };
        }
    }
}
