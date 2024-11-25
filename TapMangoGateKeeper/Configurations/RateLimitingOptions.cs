namespace TapMangoGatekeeper.Configurations
{
    public class RateLimitingOptions
    {
        public int MaxMessagesPerPhoneNumber { get; set; }
        public int MaxMessagesPerAccount { get; set; }
    }
}
