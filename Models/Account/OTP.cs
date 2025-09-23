namespace ScamSentinel.Models.Account
{
    public class OTP
    {
        public int OTPID { get; set; }
        public int UserID { get; set; }
        public string OTPCode { get; set; }
        public DateTime ExpiresAt { get; set; }
        public bool IsUsed { get; set; } = false;
    }
}