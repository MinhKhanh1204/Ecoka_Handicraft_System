using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FeedbackAPI.Models
{
    [Table("FEEDBACKS")]
    public class Feedback
    {
        [Key]
        public int FeedbackID { get; set; }

        [Required]
        public string CustomerID { get; set; } = null!;

        [Required]
        public string ProductID { get; set; } = null!;

        [Range(1, 5)]
        public int Rating { get; set; }

        public string? Comment { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }

        public string Status { get; set; } = "Active";

    }
}
