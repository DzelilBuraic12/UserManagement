namespace UserManagement.DTOs
{
    public class AuthResultDto
    {
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
        public DateTime? ExpiresAt { get; set; }
    }
    
    public class User
    {
        public int Id { get; set; } 
        public string Name { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
    }
}
