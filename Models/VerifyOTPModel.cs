namespace ScamSentinel.Models
{
    public class VerifyOTPModel
    {
        public string Email { get; set; }
        public string OTPCode { get; set; }
        public string ErrorMessage { get; internal set; }
    }
}