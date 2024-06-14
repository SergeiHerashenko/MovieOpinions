using MovieOpinions.Domain.Entity.Actors;
using MovieOpinions.Domain.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MovieOpinions.Service.Interfaces
{
    public interface IActorService
    {
        Task<BaseResponse<DetailedActor>> GetActorById(int IdActor);
    }
}
