namespace UserManagement.Domain.Entities
{
    public class PasswordResetToken
    {
        public int Id { get; set; }
        public string Token { get; set; } 
        public int UserId { get; set; }
        public User User { get; set; }
        public DateTime ExpiresAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsUsed { get; set; }
    }

}
