namespace DynamiteAndy.DynamiteToolbar.Services;

public interface IPubSubService
{
    void Subscribe<T>(Action<T> handler);
    void Unsubscribe<T>(Action<T> handler);
    void Raise<T>(T payload);
}