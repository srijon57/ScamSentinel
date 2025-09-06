namespace ScamSentinel.Models.Scam
{
    public class Vote
    {
        public int VoteID { get; set; }
        public int UserID { get; set; }
        public int ReportID { get; set; }
        public bool IsUpvote { get; set; }
        public DateTime VotedAt { get; set; } = DateTime.UtcNow;
    }
}