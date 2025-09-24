namespace ScamSentinel.Models.Scam
{
    public class ScamVerificationModel
    {
        public int ReportID { get; set; }
        public string Title { get; set; }
        public bool IsVerified { get; set; }
        public string VerificationNotes { get; set; }
    }
}