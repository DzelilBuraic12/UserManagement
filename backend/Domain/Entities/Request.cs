using System.ComponentModel.DataAnnotations;

namespace UserManagement.Domain.Entities
{
    public class Request : BaseEntity
    {
        public int StatusId { get; set; }
        public int CreatedById { get; set; }
        public int? TechnicianId { get; set; }
        public User? Technician { get; set; }
        [MaxLength(200)]
        [Required]
        public string Title { get; set; }
        public RequestStatus Status { get; set; }
        public User CreatedBy { get; set; }
        [Required]
        public string Description { get; set; }
        public enum RequestPriority { Low = 0, Normal = 1, High = 2 }

        public RequestPriority Priority { get; set; } = RequestPriority.Normal;

        public DateTime? DueDate { get; set; }


    }
}
