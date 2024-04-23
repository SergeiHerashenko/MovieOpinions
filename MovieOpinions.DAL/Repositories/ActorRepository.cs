using MovieOpinions.DAL.Interface;
using MovieOpinions.Domain.Entity.Actors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MovieOpinions.DAL.Repositories
{
    public class ActorRepository : IActorRepository
    {
        public Task<bool> Create(Actor entity)
        {
            throw new NotImplementedException();
        }

        public Task<bool> Delete(Actor entity)
        {
            throw new NotImplementedException();
        }

        public Task<Actor> GetActorName(string NameActor)
        {
            throw new NotImplementedException();
        }

        public Task<Actor> Update(Actor entity)
        {
            throw new NotImplementedException();
        }
    }
}
