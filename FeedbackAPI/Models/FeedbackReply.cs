using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FeedbackAPI.Models
{
    [Table("FEEDBACK_REPLIES")]
    public class FeedbackReply
    {
        [Key]
        public int ReplyID { get; set; }

        public int FeedbackID { get; set; }

        [Required]
        public string StaffID { get; set; } = null!;

        [Required]
        public string ReplyContent { get; set; } = null!;

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }

        // Navigation
        [ForeignKey("FeedbackID")]
        public Feedback? Feedback { get; set; }
    }
}
