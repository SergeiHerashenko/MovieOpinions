using MovieOpinions.Domain.Entity.Comments;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MovieOpinions.Domain.Entity
{
    public class Film : Films
    {
        public List<Comment> CommentFilm { get; set; }
        public List<Answer> AnswerFilm { get; set; }
    }
}
