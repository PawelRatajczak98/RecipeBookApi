using System.Text.Json.Serialization;

namespace Domain.Entities
{
    public class RecipeIngredient
    {
        public int RecipeId { get; set; }
        public Recipe Recipe { get; set; }
        public int IngredientId { get; set;}
        public Ingredient Ingredient { get; set; }
        public decimal Quantity { get; set; }
        public string Unit { get; set; }       
    }
}
