using System.ComponentModel.DataAnnotations;


namespace UserManagement.DTOs
{
    public class RequestCreateDto
    {
        [Required]
        [MaxLength(200)]
        public string Title { get; set; }
        [Required]
        public string Description { get; set; }
        public string Priority { get; set; }
        public DateTime? DueDate { get; set; }
    }
}
