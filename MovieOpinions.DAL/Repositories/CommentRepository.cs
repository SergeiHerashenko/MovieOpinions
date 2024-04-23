using MovieOpinions.DAL.Interface;
using MovieOpinions.Domain.Entity.Comments;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MovieOpinions.DAL.Repositories
{
    public class CommentRepository : ICommentRepository
    {
        public Task<bool> Create(Comment entity)
        {
            throw new NotImplementedException();
        }

        public Task<bool> Delete(Comment entity)
        {
            throw new NotImplementedException();
        }

        public Task<Comment> GetCommentId(int id)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<Comment>> GetCommentsUser(int idUser)
        {
            throw new NotImplementedException();
        }

        public Task<Comment> Update(Comment entity)
        {
            throw new NotImplementedException();
        }
    }
}
