using System;
using TapMangoGatekeeper.Configurations;
using TapMangoGatekeeper.Models;
using TapMangoGatekeeper.Services;
using Xunit;

namespace TapMangoGatekeeper.Tests.Services
{
    public class RateLimitServiceTests
    {
        private RateLimitService CreateService(RateLimitingOptions options)
        {
            return new RateLimitService(options.MaxMessagesPerPhoneNumber, options.MaxMessagesPerAccount);
        }

        [Fact]
        public void CanSend_ShouldAllowMessage_WhenWithinLimits()
        {
           
            var options = new RateLimitingOptions
            {
                MaxMessagesPerPhoneNumber = 10,
                MaxMessagesPerAccount = 50
            };
            var service = CreateService(options);
            var request = new SmsRequest { PhoneNumber = "1234567890", Message = "Test message" };

        
            var result = service.CanSend(request);

         
            Assert.True(result);
        }

        [Fact]
        public void CanSend_ShouldDenyMessage_WhenPerPhoneNumberLimitExceeded()
        {
        
            var options = new RateLimitingOptions
            {
                MaxMessagesPerPhoneNumber = 1,
                MaxMessagesPerAccount = 50
            };
            var service = CreateService(options);
            var request = new SmsRequest { PhoneNumber = "1234567890", Message = "Test message" };

           
            service.CanSend(request); // First message
            var result = service.CanSend(request); // Second message

          
            Assert.False(result);
        }

        [Fact]
        public void CanSend_ShouldDenyMessage_WhenPerAccountLimitExceeded()
        {
         
            var options = new RateLimitingOptions
            {
                MaxMessagesPerPhoneNumber = 10,
                MaxMessagesPerAccount = 1
            };
            var service = CreateService(options);
            var request1 = new SmsRequest { PhoneNumber = "1234567890", Message = "Test message 1" };
            var request2 = new SmsRequest { PhoneNumber = "0987654321", Message = "Test message 2" };

          
            service.CanSend(request1); // First message
            var result = service.CanSend(request2); // Second message

           
            Assert.False(result);
        }

        [Fact]
        public void CanSend_ShouldResetLimits_AfterInterval()
        {
          
            var options = new RateLimitingOptions
            {
                MaxMessagesPerPhoneNumber = 1,
                MaxMessagesPerAccount = 50
            };
            var service = CreateService(options);
            var request = new SmsRequest { PhoneNumber = "1234567890", Message = "Test message" };

   
            service.CanSend(request); // First message
            System.Threading.Thread.Sleep(1100); // Wait for reset interval
            var result = service.CanSend(request); // Second message

           
            Assert.True(result);
        }
    }
}
