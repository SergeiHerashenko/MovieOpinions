using MovieOpinions.DAL.Interface;
using MovieOpinions.Domain.Entity.Actors;
using MovieOpinions.Domain.Response;
using MovieOpinions.Service.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MovieOpinions.Service.Implementations
{
    public class ActorService : IActorService
    {
        private readonly IActorRepository _actorRepository;

        public ActorService(IActorRepository actorRepository)
        {
            _actorRepository = actorRepository;
        }

        public async Task<BaseResponse<DetailedActor>> GetActorById(int idActor)
        {
            var getActor = await _actorRepository.GetActorId(idActor);

            if (getActor == null)
            {
                return new BaseResponse<DetailedActor>()
                {
                    Description = "Актор не знайдений",
                    StatusCode = Domain.Enum.StatusCode.NotFound
                };
            }
            else
            {
                return new BaseResponse<DetailedActor>()
                {
                    StatusCode = Domain.Enum.StatusCode.OK,
                    Data = getActor
                };
            }
        }
    }
}
