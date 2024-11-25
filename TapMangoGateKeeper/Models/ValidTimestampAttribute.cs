using System;
using System.ComponentModel.DataAnnotations;

namespace TapMangoGatekeeper.Models
{
    public class ValidTimestampAttribute : ValidationAttribute
    {
        public override bool IsValid(object value)
        {
            if (value is DateTime timestamp)
            {
                return timestamp <= DateTime.UtcNow;
            }
            return false;
        }
    }
}
