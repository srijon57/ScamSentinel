namespace ScamSentinel.Models.Scam
{
    public class UserVotes
    {
        public int VoteID { get; set; }
        public int UserID { get; set; }
        public int ReportID { get; set; }
        public string VoteType { get; set; } // 'up' or 'down'
        public DateTime CreatedAt { get; set; }
    }
}
