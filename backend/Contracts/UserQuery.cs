namespace UserManagement.Contracts
{
    public class UserQuery
    {
        public string? Search { get; set; }
        public string? Role { get; set; }
        public bool? IsActive { get; set; }
        public bool? IsDeleted { get; set; }
        public int? Page { get; set; } = 1;
        public int? PageSize { get; set; } = 20;
        public string? SortBy { get; set; }
        public string? SortDir { get; set; }

    }
}
