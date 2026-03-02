using System.ComponentModel.DataAnnotations;

namespace FeedbackAPI.DTOs
{
    public class FeedbackReadDto
    {
        public int FeedbackID { get; set; }
        public string? CustomerID { get; set; }
        public string? ProductID { get; set; }
        public int Rating { get; set; }
        public string? Comment { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? Status { get; set; }
        
    }

    public class FeedbackCreateDto
    {
        [Required]
        public string CustomerID { get; set; } = null!;

        [Required]
        public string ProductID { get; set; } = null!;

        [Required]
        [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5.")]
        public int Rating { get; set; }

        public string? Comment { get; set; }
    }

    public class FeedbackUpdateDto
    {
        [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5.")]
        public int? Rating { get; set; }

        public string? Comment { get; set; }

        public string? Status { get; set; }
    }

    public class FeedbackFilterDto
    {
        public string? CustomerID { get; set; }
        public string? ProductID { get; set; }
        public int? MinRating { get; set; }
        public int? MaxRating { get; set; }
        public string? Status { get; set; }
        public DateTime? From { get; set; }
        public DateTime? To { get; set; }
    }

}
