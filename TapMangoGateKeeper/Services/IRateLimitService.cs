using TapMangoGatekeeper.Models;

namespace TapMangoGatekeeper.Services
{
    public interface IRateLimitService
    {
        bool CanSend(SmsRequest request);
    }
}
