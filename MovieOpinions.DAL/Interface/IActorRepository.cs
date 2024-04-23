using MovieOpinions.Domain.Entity.Actors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MovieOpinions.DAL.Interface
{
    public interface IActorRepository : IBaseRepository<Actor>
    {
        Task<Actor> GetActorName(string NameActor);
    }
}
