namespace ScamSentinel.Models.Scam
{
    public class ScamDetailsViewModel
    {
        public int ReportID { get; set; }
        public string Title { get; set; }
        public string ScamTypeName { get; set; }
        public string Description { get; set; }
        public ScammerInfo ScammerInfo { get; set; } = new ScammerInfo();
        public decimal? LossAmount { get; set; }
        public string Currency { get; set; }
        public string Location { get; set; }
        public DateTime? OccurrenceDate { get; set; }
        public bool IsVerified { get; set; }
        public DateTime CreatedAt { get; set; }
        public int Upvotes { get; set; }
        public int Downvotes { get; set; }
        public List<string> EvidenceLinks { get; set; } = new List<string>();
        public string ReporterName { get; set; }
    }
}
