using System;
using System.Collections.Generic;

namespace TapMangoGateKeeper.Models
{
    public class AccountStats
    {
        public string AccountId { get; set; }
        public int TotalMessagesSent { get; set; }
        public int MessagesSentToday { get; set; }
        public List<MessageDetails> Messages { get; set; } = new List<MessageDetails>();
        public List<DateTime> Timestamps { get; set; } = new List<DateTime>();
    }

    public class MessageDetails
    {
        public string PhoneNumber { get; set; }
        public string Message { get; set; }
        public DateTime Timestamp { get; set; }
        public string AccountId { get; set; } 
    }
}
