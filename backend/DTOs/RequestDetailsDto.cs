namespace UserManagement.DTOs
{
    public class RequestDetailsDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string StatusName { get; set; }
        public string Priority { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? DueDate { get; set; }
        public string CreatedByName { get; set; }
        public string? TechnicianName { get; set; }
        public int CreatedById { get; set; }
        public int? TechnicianId { get; set; }
    }
}
