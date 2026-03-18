using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace FeedbackAPI.Models
{
    [Table("FEEDBACK_IMAGES")]
    public class FeedbackImage
    {
        [Key]
        public int ImageID { get; set; }

        [Required]
        public int FeedbackID { get; set; }

        [Required]
        public string ImageURL { get; set; } = null!;

        [ForeignKey("FeedbackID")]
        public virtual Feedback? Feedback { get; set; }
    }
}
