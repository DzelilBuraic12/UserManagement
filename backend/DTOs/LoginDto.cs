using System.ComponentModel.DataAnnotations;

namespace UserManagement.DTOs
{
    public class LoginDto
    {
        [EmailAddress]
        [Required]
        [MaxLength(100)]
        public string Email { get; set; }
        [Required]
        public string Password { get; set; }
    }
}
