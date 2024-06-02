using MovieOpinions.Domain.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MovieOpinions.Service.Interfaces
{
    public interface IGenreService
    {
        Task<BaseResponse<IEnumerable<string>>> GetAllGenre();
    }
}
