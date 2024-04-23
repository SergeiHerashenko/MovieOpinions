using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MovieOpinions.DAL.Interface
{
    public interface IGenreRepository
    {
        Task<IEnumerable<string>> GetGenre();
    }
}
