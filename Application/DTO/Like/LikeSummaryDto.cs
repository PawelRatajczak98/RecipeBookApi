using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTO.Like
{
    public class LikeSummaryDto
    {
        public int TotalLikes { get; set; }
        public bool LikedByCurrentUser { get; set; }
    }
}
