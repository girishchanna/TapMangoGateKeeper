using System;
using System.ComponentModel.DataAnnotations;

namespace TapMangoGatekeeper.Models
{
    public class SmsRequest
    {
        [Required]
        [RegularExpression(@"^\+1\d{10}$", ErrorMessage = "Phone number must be a valid Canadian phone number")]
        public string PhoneNumber { get; set; }

        [Required]
        [MaxLength(300, ErrorMessage = "Message cannot exceed 300 characters.")]
        public string Message { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Account ID must be a valid integer.")]
        public string AccountId { get; set; }

        [Required]
        [ValidTimestamp(ErrorMessage = "Timestamp must not be in the future.")]
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
