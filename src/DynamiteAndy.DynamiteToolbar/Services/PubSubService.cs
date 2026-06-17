using System.Collections.Concurrent;
using System.Collections.Immutable;

namespace DynamiteAndy.DynamiteToolbar.Services;

public sealed class PubSubService : IPubSubService, IDisposable
{
    private readonly ConcurrentDictionary<Type, ImmutableList<Delegate>> _handlers = new();

    public void Subscribe<T>(Action<T> handler)
    {
        _handlers.AddOrUpdate(
            typeof(T),
            [handler],
            (_, existingList) => existingList.Add(handler)
        );
    }

    public void Unsubscribe<T>(Action<T> handler)
    {
        var type = typeof(T);

        while (_handlers.TryGetValue(type, out var oldList))
        {
            var newList = oldList.Remove(handler);

            if (newList.IsEmpty)
            {
                if (_handlers.TryRemove(new KeyValuePair<Type, ImmutableList<Delegate>>(type, oldList)))
                {
                    break;
                }
            }
            else
            {
                if (_handlers.TryUpdate(type, newList, oldList))
                {
                    break;
                }
            }
        }
    }

    public void Raise<T>(T payload)
    {
        if (!_handlers.TryGetValue(typeof(T), out var list))
        {
            return;
        }

        foreach (var handler in list)
        {
            ((Action<T>)handler).Invoke(payload);
        }
    }

    public void Dispose()
    {
        _handlers.Clear();
    }
}