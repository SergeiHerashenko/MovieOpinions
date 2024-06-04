using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MovieOpinions.Domain.Entity.Comments
{
    public class Answer
    {
        public int IdAnswer { get; set; }
        public int IdComment { get; set; }
        public string TextAnswer { get; set; }
        public int IdUserAnswer { get; set; }
        public string NameUserAnswer { get; set; }
    }
}
