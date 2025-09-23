namespace ScamSentinel.Models
{
    public class ScamCheckResponse
    {
        public bool IsFraud { get; set; }
        public string Message { get; set; } = string.Empty;
        public object? Raw { get; set; } // optional: include raw VirusTotal response summary if you want
    }
}
