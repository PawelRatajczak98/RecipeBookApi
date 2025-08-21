using System.Text.Json.Serialization;

namespace Domain.Entities
{
    public class Recipe
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }     
        public ICollection<RecipeIngredient> RecipeIngredients { get; set; } = new List<RecipeIngredient>();
        public ICollection<Category> Categories { get; set; } = new List<Category>();
        public ICollection<Comment> Comments { get; set; } = new List<Comment>();
        public ICollection<Like> Likes { get; set; } = new List<Like>();
        public decimal TotalCost { get; set; } = 0m;
        public ICollection<Rating> Ratings { get; set; } = new List<Rating>();
        public decimal AverageRating =>
            Ratings.Count > 0 ? Math.Round(Ratings.Average(r =>(decimal) r.Value), 2) : 0m;

        public string Instructions { get; set; }
        public TimeSpan PreparationTime { get; set; }
        public TimeSpan CookingTime { get; set; }
        public TimeSpan TotalTime { get; set; }
        public int Servings { get; set; }
        public string AuthorUserName { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? ImageUrl { get; set; }
            
    }
}
