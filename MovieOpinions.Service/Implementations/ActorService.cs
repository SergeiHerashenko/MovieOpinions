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

        public async Task<BaseResponse<DetailedActor>> GetActorById(int IdActor)
        {
            var GetActor = await _actorRepository.GetActorId(IdActor);

            if(GetActor.Data != null)
            {
                DetailedActor actor = new DetailedActor()
                {
                    IdActor = GetActor.Data.IdActor,
                    LastName = GetActor.Data.LastName,
                    FirstName = GetActor.Data.FirstName,
                    BirthdayActor = GetActor.Data.BirthdayActor,
                    FilmActor = GetActor.Data.FilmActor,
                    GenreActor = GetActor.Data.GenreActor,
                    CountryActor = GetActor.Data.CountryActor,
                    ActorImage = GetActor.Data.ActorImage,
                };

                return new BaseResponse<DetailedActor>()
                {
                    StatusCode = Domain.Enum.StatusCode.OK,
                    Data = actor
                };
            }
            else
            {
                if(GetActor.StatusCode == Domain.Enum.StatusCode.OK)
                {
                    return new BaseResponse<DetailedActor>()
                    {
                        Description = "Актор не знайдений",
                        StatusCode = GetActor.StatusCode
                    };
                }
                else
                {
                    return new BaseResponse<DetailedActor>()
                    {
                        Description = GetActor.Description,
                        StatusCode = GetActor.StatusCode
                    };
                }
            }
        }
    }
}
