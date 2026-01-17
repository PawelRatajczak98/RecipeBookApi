using System.ComponentModel.DataAnnotations;

namespace Application.DTO.Ingredient
{
    public class IngredientCreateDto
    {
        public string Name { get; set; }

        public string Description { get; set; }

        public decimal PriceFor100Grams { get; set; }

        public decimal? PriceForSingle { get; set; }

        public string Unit { get; set;  }
    }
}
