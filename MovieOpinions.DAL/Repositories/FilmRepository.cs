using MovieOpinions.DAL.Interface;
using MovieOpinions.Domain.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MovieOpinions.DAL.Repositories
{
    public class FilmRepository : IFilmRepository
    {
        public Task<bool> Create(Film entity)
        {
            throw new NotImplementedException();
        }

        public Task<bool> Delete(Film entity)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<Film>> GetAll()
        {
            throw new NotImplementedException();
        }

        public Task<Film> GetMovieGenre(string Genre)
        {
            throw new NotImplementedException();
        }

        public Task<Film> GetMovieId(int id)
        {
            throw new NotImplementedException();
        }

        public Task<Film> GetMovieName(string Name)
        {
            throw new NotImplementedException();
        }

        public Task<Film> GetMovieYear(int year)
        {
            throw new NotImplementedException();
        }

        public Task<Film> Update(Film entity)
        {
            throw new NotImplementedException();
        }
    }
}
