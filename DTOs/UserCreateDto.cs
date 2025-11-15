using System.ComponentModel.DataAnnotations;

namespace UserManagement.DTOs
{
    public class UserCreateDto
    {
        [MinLength(3)]
        [MaxLength(100)]
        [Required]
        public string FirstName { get; set; }
        [MinLength(3)]
        [MaxLength(100)]
        [Required]
        public string LastName { get; set; }
        [Required]
        [StringLength(256)]
        [EmailAddress]
        public string Email { get; set; }
        [Required]
        [MinLength(8)]
        public string Password { get; set; }
        public string Role { get; set; } = "User";
        public bool? IsActive { get; set; } = true;

    }
}
