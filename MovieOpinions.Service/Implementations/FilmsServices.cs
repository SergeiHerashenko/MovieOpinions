using MovieOpinions.DAL.Interface;
using MovieOpinions.Domain.Entity;
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

        public async Task<List<Films>> GetFilms()
        {
            return await _filmRepository.GetAll();
        }
    }
}
