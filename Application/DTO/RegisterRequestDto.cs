using System.ComponentModel.DataAnnotations;

namespace Application.DTO
{
    public class RegisterRequestDto
    {
        [Required]
        public string UserName { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        [StringLength(16,MinimumLength = 4)]
        public string Password { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; } = string.Empty;

    }
}
