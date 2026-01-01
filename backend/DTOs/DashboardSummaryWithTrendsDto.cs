namespace UserManagement.DTOs
{
    public class DashboardSummaryWithTrendsDto
    {
        public int Open { get; set; }
        public int InProgress { get; set; }
        public int Resolved { get; set; }
        public int Closed { get; set; }
        public TrendsDto Trends { get; set; }
    }

    public class TrendsDto
    {
        public int Open { get; set; }
        public int InProgress { get; set; }
        public int Resolved { get; set; }
        public int Closed { get; set; }
    }
}
