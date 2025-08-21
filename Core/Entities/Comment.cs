namespace Domain.Entities
{
    public class Comment
    {
        public string Content { get; set; }
        public string UserId { get; set; }
        public AppUser User { get; set; }
        public int RecipeId { get; set; }
        public Recipe Recipe { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}