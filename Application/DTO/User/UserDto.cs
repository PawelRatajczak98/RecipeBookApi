using Application.DTO.UserIngredient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTO.User
{
    public class UserDto
    {
        public string UserId { get; set; }
        public string UserName { get; set; }
        public decimal? Budget { get; set; }
        public List<UserIngredientDto> UserIngredients { get; set; } = new List<UserIngredientDto>();
    }
}
