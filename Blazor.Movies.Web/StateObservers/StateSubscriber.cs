namespace Blazor.Movies.Web.StateObservers;

public class StateSubscriber(string stateName, object stateObject): IDisposable
{
    public string StateName { get; } = stateName;
    public object StateObject { get; set; } = stateObject;
    
    public StateEventHandler? StateHandler;
    
    public delegate void StateEventHandler(object state);

    public void Dispose()
    {
        if(StateHandler == null) return;
        Delegate[] invocationList = StateHandler.GetInvocationList();
        foreach (var @delegate in invocationList)
        {
            var handler = (StateEventHandler)@delegate;
            StateHandler -= handler;
        }
    }

    ~StateSubscriber()
    {
        Dispose();
    }
}
