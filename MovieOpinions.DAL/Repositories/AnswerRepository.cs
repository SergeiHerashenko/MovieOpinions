using MovieOpinions.DAL.Interface;
using MovieOpinions.Domain.Entity.Comments;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MovieOpinions.DAL.Repositories
{
    public class AnswerRepository : IAnswerRepository
    {
        public Task<bool> Create(Answer entity)
        {
            throw new NotImplementedException();
        }

        public Task<bool> Delete(Answer entity)
        {
            throw new NotImplementedException();
        }

        public Task<Answer> GetAnswerId(int id)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<Answer>> GetAnswerUser(int idUser)
        {
            throw new NotImplementedException();
        }

        public Task<Answer> Update(Answer entity)
        {
            throw new NotImplementedException();
        }
    }
}
