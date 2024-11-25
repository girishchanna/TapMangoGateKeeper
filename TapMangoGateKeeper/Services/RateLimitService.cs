using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using TapMangoGatekeeper.Models;

namespace TapMangoGatekeeper.Services
{
    public class RateLimitService : IRateLimitService
    {
        private readonly int _maxMessagesPerPhoneNumber;
        private readonly int _maxMessagesPerAccount;
        private readonly ConcurrentDictionary<string, RateLimitTracker> _phoneNumberTrackers;
        private readonly RateLimitTracker _accountTracker;
        private readonly TimeSpan _expirationTime = TimeSpan.FromMinutes(1);

        public RateLimitService(int maxMessagesPerPhoneNumber, int maxMessagesPerAccount)
        {
            _maxMessagesPerPhoneNumber = maxMessagesPerPhoneNumber;
            _maxMessagesPerAccount = maxMessagesPerAccount;
            _phoneNumberTrackers = new ConcurrentDictionary<string, RateLimitTracker>();
            _accountTracker = new RateLimitTracker();
            Task.Run(CleanUpExpiredEntries);
        }

        public bool CanSend(SmsRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.PhoneNumber))
                return false;

            if (_accountTracker.Count >= _maxMessagesPerAccount)
                return false;

            var phoneTracker = _phoneNumberTrackers.GetOrAdd(request.PhoneNumber, new RateLimitTracker());
            if (phoneTracker.Count >= _maxMessagesPerPhoneNumber)
                return false;

            phoneTracker.Increment();
            _accountTracker.Increment();
            return true;
        }

        private void CleanUpExpiredEntries()
        {
            while (true)
            {
                var now = DateTime.UtcNow;
                var expiredKeys = _phoneNumberTrackers.Where(kvp => kvp.Value.IsExpired(_expirationTime)).Select(kvp => kvp.Key).ToList();

                foreach (var key in expiredKeys)
                {
                    _phoneNumberTrackers.TryRemove(key, out _);
                }

                Task.Delay(_expirationTime).Wait();
            }
        }
    }
}
