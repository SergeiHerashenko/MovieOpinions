using MovieOpinions.Domain.Entity.Actors;
using MovieOpinions.Domain.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MovieOpinions.DAL.Interface
{
    public interface IActorRepository : IBaseRepository<Actor>
    {
        Task<BaseResponse<Actor>> GetActorName(string NameActor);
        Task<BaseResponse<DetailedActor>> GetActorId(int IdActor);
    }
}
