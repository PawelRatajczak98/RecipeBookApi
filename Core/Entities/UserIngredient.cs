using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Domain.Entities
{
    public class UserIngredient
    {
        public string UserId { get; set; }
        public int IngredientId { get; set; }
        public string IngredientName { get; set; }
        public Ingredient Ingredient { get; set; }
        public decimal Quantity { get; set; }
        public AppUser User { get; set; }      
        public string Unit { get; set; }

    }
}
