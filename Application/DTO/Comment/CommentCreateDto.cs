using System.ComponentModel.DataAnnotations;

namespace Application.DTO.Comment
{
    public class CommentCreateDto
    {
        [Required(ErrorMessage = "RecipeId jest wymagane")]
        [Range(1, int.MaxValue, ErrorMessage = "RecipeId musi być większe niż 0")]
        public int RecipeId { get; set; }

        [Required(ErrorMessage = "Treść komentarza jest wymagana")]
        [MinLength(1, ErrorMessage = "Komentarz nie może być pusty")]
        [MaxLength(500, ErrorMessage = "Komentarz nie może przekraczać 500 znaków")]
        public string CommentContent { get; set; }
    }
}
