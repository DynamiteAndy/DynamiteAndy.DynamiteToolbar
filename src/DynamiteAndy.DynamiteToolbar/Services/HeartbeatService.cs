using DynamiteAndy.DynamiteToolbar.Models.Events;

namespace DynamiteAndy.DynamiteToolbar.Services;

public sealed class HeartbeatService(IPubSubService pubSub, TimeProvider timeProvider)
    : IHeartbeatService, IDisposable
{
    private ITimer? _timer;

    public void Start(TimeSpan dueTime, TimeSpan period)
    {
        _timer = timeProvider.CreateTimer(_ => OnTick(), null, dueTime, period);
    }

    public void OnTick() => pubSub.Raise(new HeartbeatEvent());

    public void Dispose()
    {
        _timer?.Dispose();
        _timer = null;
    }
}