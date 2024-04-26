using MovieOpinions.Domain.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MovieOpinions.Service.Interfaces
{
    public interface IFilmsServices
    {
        Task<List<Films>> GetFilms();
    }
}
