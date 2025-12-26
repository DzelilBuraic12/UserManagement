namespace UserManagement.DTOs
{
    public class UserListDto
    {
        public int Id { get; set; }
        public string FirstName { get; set; } = default!;
        public string LastName { get; set; } = default!;
        public string Email { get; set; } = default!;
        public string Role { get; set; } = default!;
        public bool? IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
