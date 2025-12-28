
using System.Collections.Concurrent;

namespace Blazor.Movies.Web.StateObservers;


public interface IStateManager
{
    void UpdateState(string key, object? state);
    void Subscribe(string key, StateSubscriber.StateEventHandler action);
    void Unsubscribe(string key, StateSubscriber.StateEventHandler action);
    object? GetCurrentState(string key);
}

public class StateManager : IStateManager,IDisposable
{
    private readonly ConcurrentDictionary<string, StateSubscriber>  _subscribers = new();
    public object? GetCurrentState(string key)
    {
        if(string.IsNullOrWhiteSpace(key)) throw new ArgumentNullException(nameof(key));
        return _subscribers.TryGetValue(key.ToLowerInvariant(), out var subscriber) ? subscriber.StateObject : null;
    }
    public void UpdateState(string key, object? state)
    {
        if(string.IsNullOrWhiteSpace(key)) throw new ArgumentNullException(nameof(key));
        var updated = false;
        key = key.ToLowerInvariant();
        _subscribers.TryGetValue(key, out var subscriber);
        if (subscriber == null)
        {
            subscriber = new StateSubscriber(key, state!);
            _subscribers.GetOrAdd(key, subscriber);
            updated = false;
        }
        else
        {
            if (state != null && subscriber.StateObject != null && subscriber.StateObject.GetType().Name != state.GetType().Name)
                throw new Exception($"The state type must be {subscriber.StateObject.GetType().Name}");
                
            if (state == null && subscriber.StateObject != null ||
                state != null && subscriber.StateObject == null ||
                subscriber.StateObject?.GetHashCode() != state?.GetHashCode())
            {
                subscriber.StateObject = state!;
                updated = true;
            }
        }
        if(!updated) return;
        
        //Fire subscribed events
        subscriber.StateHandler?.Invoke(subscriber.StateObject!);
    }

    public void Subscribe(string key, StateSubscriber.StateEventHandler action)
    {
        if(string.IsNullOrWhiteSpace(key)) throw new ArgumentNullException(nameof(key));
        ArgumentNullException.ThrowIfNull(action);
        key = key.ToLowerInvariant(); 
        _subscribers.TryGetValue(key, out var subscriber);
        if (subscriber == null)
        {
            subscriber =  new StateSubscriber(key, null! );
            _subscribers.GetOrAdd(key, subscriber);
        }
        //Subscribe the event once
        subscriber.StateHandler -= action;
        subscriber.StateHandler += action;
    }
    
    public void Unsubscribe(string key, StateSubscriber.StateEventHandler action)
    {
        if(string.IsNullOrWhiteSpace(key)) throw new ArgumentNullException(nameof(key));
        ArgumentNullException.ThrowIfNull(action);
        _subscribers.TryGetValue(key.ToLowerInvariant(), out var subscriber);
        if (subscriber == null) return;
        //Unsubscribe the event 
        subscriber.StateHandler -= action;
    }
   
    public void Dispose()
    {
        _subscribers.Clear();
        GC.SuppressFinalize(this);
    }
}