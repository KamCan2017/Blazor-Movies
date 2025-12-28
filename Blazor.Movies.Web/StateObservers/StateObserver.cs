
namespace Blazor.Movies.Web.StateObservers;

public interface IStateObserver
{
    /// <summary>
    /// Get the last state, in case that the subscriber was instantiated after the movie observer. 
    /// </summary>
    /// <returns>The current state.</returns>
    TSource? GetCurrentState<T,TSource>()where T: PubSubEvent<TSource>, new();

    /// <summary>
    /// Save the new state and dispatch it to the subscribers
    /// </summary>
    /// <param name="newState">The new state</param>
    /// <typeparam name="T">The event type to fire</typeparam>
    /// <typeparam name="TSource">The type of the new state</typeparam>
    void Dispatch<T,TSource>(TSource newState) where T: PubSubEvent<TSource>, new();

    /// <summary>
    /// Subscribe to the event
    /// </summary>
    /// <param name="action">The action to execute when the event is published</param>
    /// <typeparam name="T">The event type to subscribe to</typeparam>
    /// <typeparam name="TSource"> the type of the event source</typeparam>
    void Subscribe<T, TSource>(Action<TSource> action) where T : PubSubEvent<TSource>, new();
}

public class StateObserver(IEventAggregator eventAggregator) : IStateObserver
{
    private readonly IEventAggregator _eventAggregator = eventAggregator ?? throw new ArgumentNullException(nameof(eventAggregator));
    
    private readonly Dictionary<Type, object> _stateCollector = [];

   /// <summary>
   /// Get the last state, in case that the subscriber was instantiated after the movie observer. 
   /// </summary>
   /// <typeparam name="T">The event type to get the state for</typeparam>
   /// <typeparam name="TSource">The type of the state</typeparam>
   /// <returns>The current state.</returns>
    public TSource? GetCurrentState<T,TSource>()where T: PubSubEvent<TSource>, new()
    {
        return !_stateCollector.ContainsKey(typeof(T)) ? default : (TSource)_stateCollector[typeof(T)];
    }


    public void Dispatch<T,TSource>(TSource newState) where T: PubSubEvent<TSource>, new()
    {
        ArgumentNullException.ThrowIfNull(newState);
        if(!_stateCollector.ContainsKey(typeof(T)))
            _stateCollector.Add(typeof(T), newState);
        else
        {
            _stateCollector[typeof(T)] = newState;
        }
        _eventAggregator.GetEvent<T>().Publish(newState);
    }

    public void Subscribe<T, TSource>(Action<TSource> action) where T : PubSubEvent<TSource>, new()
    {
        _eventAggregator.GetEvent<T>().Subscribe(action);
    }

}