using Application.DTO.RecipeIngredient;
using Application.DTO.Comment;
using Application.DTO.Like;


namespace Application.DTO.Recipe
{
    public class RecipeDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public List<RecipeIngredientDto> RecipeIngredients { get; set; } = new List<RecipeIngredientDto>();
        public List<CommentDto> Comments { get; set; } = new List<CommentDto>();
        public List<LikeDto> Likes { get; set; } = new List<LikeDto>();
        public decimal TotalCost { get; set; }
    }
}
