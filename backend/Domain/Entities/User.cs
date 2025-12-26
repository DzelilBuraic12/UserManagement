using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace UserManagement.Domain.Entities
{

    [Index(nameof(Email), IsUnique = true)]
    public class User : BaseEntity
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        [EmailAddress]
        [Required]
        [StringLength(256)]
        public string Email { get; set; }
        [Required]
        public string PasswordHash { get; set; }
        public string Role { get; set; }
        public bool IsActive { get; set; }

        public ICollection<Request> CreatedRequests { get; set; } = [];
        public ICollection<Request> AssignedRequests { get; set; } = [];
        public List<RefreshToken> RefreshTokens { get; set; }
        public List<PasswordResetToken> PasswordResetTokens { get; set; }
    }
}
