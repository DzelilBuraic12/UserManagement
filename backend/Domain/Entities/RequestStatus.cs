using System.ComponentModel.DataAnnotations;

namespace UserManagement.Domain.Entities
{
    public class RequestStatus : BaseEntity
    {
        [Required]
        [MaxLength(50)]
        public string Name { get; set; }
        public int Order {  get; set; }

        public ICollection<Request> Requests { get; set; } = [];
    }
}
