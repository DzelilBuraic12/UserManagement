namespace UserManagement.DTOs
{
    public class UserFilterDto
    {
        int? page {  get; set; }
        int? pageSize { get; set; }
        string? search { get; set; }
        string? role { get; set; }
        bool? isActive { get; set; }
    }
}
