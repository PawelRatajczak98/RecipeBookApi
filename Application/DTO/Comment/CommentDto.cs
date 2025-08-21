using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTO.Comment
{
    public class CommentDto
    {
        public string Content { get; set; }
        public string UserName  { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
