namespace UserManagement.Contracts
{
    public class RequestQuery
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int? StatusId { get; set; }
        public int? TechnicianId { get; set; }
        public int? CreatedById { get; set; }
        public string? MyAssignedOnly { get; set; }
        public string? Search { get; set; }
        public string? SortBy { get; set; } = "CreatedAt";
        public string? SortDir { get; set; } = "desc";
        public bool? IncludeClosed { get; set; } = false;
    }
}
