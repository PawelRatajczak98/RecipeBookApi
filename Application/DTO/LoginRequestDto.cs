using System.ComponentModel.DataAnnotations;

namespace Application.DTO
{
    public class LoginRequestDto
    {
        [Required]
        [MinLength(3, ErrorMessage = "UserName must be at least 3 characters long")]
        [MaxLength(20, ErrorMessage = "UserName too long")]
        public string UserName { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }
}
