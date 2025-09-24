using System.ComponentModel.DataAnnotations;

namespace ScamSentinel.Models.Scam
{
    public class Comment
    {
        public int CommentID { get; set; }
        public int ReportID { get; set; }
        public int UserID { get; set; }
        public string CommentText { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public bool IsEdited { get; set; }
        public string UserName { get; set; } // For display purposes
    }

    public class CommentModel
    {
        [Required(ErrorMessage = "Comment text is required")]
        [StringLength(1000, ErrorMessage = "Comment cannot exceed 1000 characters")]
        public string CommentText { get; set; }

        public int ReportID { get; set; }
    }
}