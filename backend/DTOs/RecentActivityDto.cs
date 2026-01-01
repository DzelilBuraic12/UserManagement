namespace UserManagement.DTOs
{
    public class RecentActivityDto
    {
        public int Id { get; set; }
        public int TicketId { get; set; }
        public string Message { get; set; }
        public string Time {  get; set; }
        public string Type { get; set; }
    }
}
