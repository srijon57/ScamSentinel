
namespace ScamSentinel.Models.Scam
{
    public class UserPostsViewModel
    {
        public List<UserScamReport> ScamReports { get; set; } = new List<UserScamReport>();
        public int CurrentPage { get; set; } = 1;
        public int TotalPages { get; set; }
        public int PageSize { get; set; } = 10;
    }

    public class UserScamReport
    {
        public int ReportID { get; set; }
        public string Title { get; set; }
        public string ScamTypeName { get; set; }
        public string Description { get; set; }
        public DateTime CreatedAt { get; set; }
        public int Upvotes { get; set; }
        public int Downvotes { get; set; }
        public bool IsVerified { get; set; }
        public bool CanDelete { get; set; } = true; // Users can always delete their own posts
    }
}