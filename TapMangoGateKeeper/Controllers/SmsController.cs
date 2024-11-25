using Microsoft.AspNetCore.Mvc;
using TapMangoGatekeeper.Models;
using TapMangoGatekeeper.Services;
using TapMangoGateKeeper.Models;
using System.Collections.Generic;
using System.Linq;

namespace TapMangoGatekeeper.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SmsController : ControllerBase
    {
        private readonly IRateLimitService _rateLimitService;
        private static Dictionary<string, AccountStats> _accountStats = new Dictionary<string, AccountStats>();

        public SmsController(IRateLimitService rateLimitService)
        {
            _rateLimitService = rateLimitService;
        }

        [HttpPost("send")]
        public IActionResult CanSend([FromBody] SmsRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var canSend = _rateLimitService.CanSend(request);
            if (!canSend)
            {
                return StatusCode(429, new ApiResponse { Message = "Rate limit exceeded. Try again later." });
            }

            UpdateStatistics(request);

            return Ok(new ApiResponse { Message = "Message can be sent." });
        }

        private void UpdateStatistics(SmsRequest request)
        {
            if (!_accountStats.ContainsKey(request.AccountId))
            {
                _accountStats[request.AccountId] = new AccountStats { AccountId = request.AccountId, TotalMessagesSent = 0, MessagesSentToday = 0 };
            }

            var accountStats = _accountStats[request.AccountId];
            accountStats.TotalMessagesSent++;
            accountStats.MessagesSentToday++;
            accountStats.Messages.Add(new MessageDetails
            {
                PhoneNumber = request.PhoneNumber,
                Message = request.Message,
                Timestamp = request.Timestamp,
                AccountId = request.AccountId // Populate AccountId
            });
            accountStats.Timestamps.Add(request.Timestamp);

            Stats.TotalMessagesSent++;
            Stats.MessagesSentToday++;
        }


        [HttpGet("stats")]
        public IActionResult GetStats()
        {
            var stats = new
            {
                TotalMessagesSent = Stats.TotalMessagesSent,
                MessagesSentToday = Stats.MessagesSentToday
            };

            return Ok(stats);
        }

        [HttpGet("stats/account/{accountId}")]
        public IActionResult GetStatsByAccount(string accountId)
        {
            if (_accountStats.ContainsKey(accountId))
            {
                return Ok(_accountStats[accountId]);
            }

            return NotFound(new ApiResponse { Message = "Account not found." });
        }

        [HttpGet("stats/messagesPerSecond")]
        public IActionResult GetMessagesPerSecond()
        {
            var now = DateTime.UtcNow;
            var pastInterval = TimeSpan.FromSeconds(5); //  interval to calculate average messages per second

            var messagesInInterval = _accountStats.Values
                                                  .SelectMany(a => a.Timestamps)
                                                  .Where(t => (now - t) <= pastInterval)
                                                  .ToList();

            var intervalInSeconds = pastInterval.TotalSeconds;
            var messagesPerSecond = messagesInInterval.Count / intervalInSeconds;

            return Ok(new { MessagesPerSecond = messagesPerSecond });
        }


        [HttpGet("stats/messages")]
        public IActionResult GetFilteredMessages([FromQuery] string accountId, [FromQuery] string phoneNumber, [FromQuery] string interval)
        {
            var now = DateTime.UtcNow;
            var filteredMessages = _accountStats.Values.SelectMany(a => a.Messages.AsEnumerable());

            if (!string.IsNullOrEmpty(accountId) && accountId != "all")
            {
                filteredMessages = filteredMessages.Where(m => m.AccountId == accountId);
            }

            if (!string.IsNullOrEmpty(phoneNumber) && phoneNumber != "all")
            {
                filteredMessages = filteredMessages.Where(m => m.PhoneNumber == phoneNumber);
            }

            switch (interval?.ToLower())
            {
                case "second":
                    filteredMessages = filteredMessages.Where(m => (now - m.Timestamp).TotalSeconds <= 1);
                    break;
                case "5minutes":
                    filteredMessages = filteredMessages.Where(m => (now - m.Timestamp).TotalMinutes <= 5);
                    break;
                case "1hour":
                    filteredMessages = filteredMessages.Where(m => (now - m.Timestamp).TotalHours <= 1);
                    break;
            }

            return Ok(filteredMessages.ToList());
        }

        [HttpGet("stats/phonenumbermessages")]
        public IActionResult GetFilteredPhoneNumberMessages([FromQuery] string phoneNumber, [FromQuery] DateTime? startDateTime, [FromQuery] DateTime? endDateTime)
        {
            var filteredMessages = _accountStats.Values.SelectMany(a => a.Messages.AsEnumerable());

            if (!string.IsNullOrEmpty(phoneNumber) && phoneNumber != "all")
            {
                filteredMessages = filteredMessages.Where(m => m.PhoneNumber == phoneNumber);
            }

            if (startDateTime.HasValue)
            {
                filteredMessages = filteredMessages.Where(m => m.Timestamp >= startDateTime.Value);
            }

            if (endDateTime.HasValue)
            {
                filteredMessages = filteredMessages.Where(m => m.Timestamp <= endDateTime.Value);
            }

            return Ok(filteredMessages.ToList());
        }

        [HttpGet("stats/allPhoneNumbersAndMessages")]
        public IActionResult GetAllPhoneNumbersAndMessages()
        {
            var phoneNumbersAndMessages = _accountStats.Values
                                                       .SelectMany(a => a.Messages.Select(m => new
                                                       {
                                                           m.PhoneNumber,
                                                           m.Message,
                                                           m.Timestamp,
                                                           m.AccountId
                                                       }))
                                                       .Distinct()
                                                       .ToList();

            return Ok(phoneNumbersAndMessages);
        }

        [HttpGet("accounts")]
        public IActionResult GetAccounts()
        {
            var accounts = _accountStats.Keys.ToList();
            return Ok(accounts);
        }

        [HttpGet("phoneNumbers")]
        public IActionResult GetPhoneNumbers()
        {
            var phoneNumbers = _accountStats.Values
                                    .SelectMany(a => a.Messages)
                                    .Select(m => m.PhoneNumber)
                                    .Distinct()
                                    .ToList();
            return Ok(phoneNumbers);
        }

        [HttpGet("stats/messagesByDateRange")]
        public IActionResult GetMessagesByDateRange([FromQuery] string accountId, [FromQuery] DateTime? startDateTime, [FromQuery] DateTime? endDateTime)
        {
            var filteredMessages = _accountStats.Values.SelectMany(a => a.Messages.AsEnumerable());

            if (!string.IsNullOrEmpty(accountId) && accountId != "all")
            {
                filteredMessages = filteredMessages.Where(m => m.AccountId == accountId);
            }

            if (startDateTime.HasValue)
            {
                filteredMessages = filteredMessages.Where(m => m.Timestamp >= startDateTime.Value);
            }

            if (endDateTime.HasValue)
            {
                filteredMessages = filteredMessages.Where(m => m.Timestamp <= endDateTime.Value);
            }

            return Ok(filteredMessages.ToList());
        }

    }
}
