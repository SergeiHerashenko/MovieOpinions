using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MovieOpinions.Domain.Entity.Comments
{
    public class Comment
    {
        public int IdComment { get; set; }
        public int IdUserComment { get; set; }
        public string UserName { get; set; }
        public string TextComment { get; set; }
        public int IdFilm { get; set; }
        public DateTime DateComment { get; set; }
        public List<Answer> AnswerComment { get; set; } = new List<Answer>();
    }
}
