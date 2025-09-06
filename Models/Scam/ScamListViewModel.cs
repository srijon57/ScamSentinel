namespace ScamSentinel.Models.Scam
{
    public class ScamListViewModel
    {
        public List<ScamReport> ScamReports { get; set; } = new List<ScamReport>();
        public int CurrentPage { get; set; } = 1;
        public int TotalPages { get; set; }
        public int PageSize { get; set; } = 7;
        public string SearchTerm { get; set; }
        public int? ScamTypeFilter { get; set; }
        public List<ScamType> AvailableScamTypes { get; set; } = new List<ScamType>();
    }

    public class ScamReport
    {
        public int ReportID { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public ScammerInfo ScammerInfo { get; set; } = new ScammerInfo(); // Initialize to avoid null
        public int ScamTypeID { get; set; }
        public string ScamTypeName { get; set; }
        public int Upvotes { get; set; }
        public int Downvotes { get; set; }
        public DateTime CreatedAt { get; set; }
        public int? UserVote { get; set; } // 1 for upvote, -1 for downvote, null for no vote
    }
}