using System;

namespace TapMangoGatekeeper.Models
{
    public class RateLimitTracker
    {
        public int Count { get; set; }
        public DateTime LastUpdated { get; set; }

        public RateLimitTracker()
        {
            Count = 0;
            LastUpdated = DateTime.UtcNow;
        }

        public void Increment()
        {
            Count++;
            LastUpdated = DateTime.UtcNow;
        }

        public bool IsExpired(TimeSpan expirationTime)
        {
            return DateTime.UtcNow - LastUpdated > expirationTime;
        }
    }
}
