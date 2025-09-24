using ScamSentinel.Models.Scam;

namespace ScamSentinel.Models.Account
{
    public class User
    {
        public int UserID { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public string ContactNumber { get; set; }
        public bool IsVerified { get; set; }
        public bool SuperUser { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation property for user's scam reports
        public List<ScamReport> ScamReports { get; set; } = new List<ScamReport>();
    }
}