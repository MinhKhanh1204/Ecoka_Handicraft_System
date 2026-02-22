using System.ComponentModel.DataAnnotations;

namespace AccountAPI.DTOs
{
    public class LoginRequestDto
    {
        [Required]
        public string Username { get; set; } = null!;

        [Required]
        [MinLength(3)]
        public string Password { get; set; } = null!;
    }

}
