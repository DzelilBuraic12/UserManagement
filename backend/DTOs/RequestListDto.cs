namespace UserManagement.DTOs
{
    public class RequestListDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string StatusName { get; set; }
        public string Priority { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? TechnicianName { get; set; }
    }
}
