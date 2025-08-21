using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class Rating
    {
        public string UserId { get; set; }
        public int RecipeId { get; set; }
        public int Value { get; set; }
        public string? Comment { get; set; }        
        public Recipe Recipe { get; set; }
        public AppUser User { get; set; }
    }
}
