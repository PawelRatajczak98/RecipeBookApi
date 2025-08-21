using Application.DTO.RecipeIngredient;

namespace Application.DTO.Recipe
{
    public class RecipeCreateDto
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public List<RecipeIngredientDto> RecipeIngredientsDto { get; set; }
        public string Instructions { get; set; }
        public TimeSpan PreparationTime { get; set; }
        public TimeSpan CookingTime { get; set; }
        public int Servings { get; set; }
    }
}
