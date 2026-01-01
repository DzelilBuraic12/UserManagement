namespace UserManagement.DTOs
{
    public class RequestSummaryDto
    {
        public int Open { get; set; }
        public int InProgress { get; set; }
        public int Resolved { get; set; }
        public int Closed { get; set; }
    }
}
