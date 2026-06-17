namespace DynamiteAndy.DynamiteToolbar.Services;

public interface IHeartbeatService
{
    void Start(TimeSpan due, TimeSpan period);
    void OnTick();
}