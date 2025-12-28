using Blazor.Movies.Web.StateObservers;

namespace Blazor.Movies.Web.Tests.StateObservers;
/// <summary>
/// A concrete event implementation used for testing state changes of type 'string'.
/// It satisfies the 'where T: PubSubEvent<TSource>, new()' constraint.
/// </summary>
public class StringStateChangedEvent : PubSubEvent<string> { }

// A separate event type for testing different streams of data
public class IntStateChangedEvent : PubSubEvent<int> { }

[TestFixture]
public class StateObserverTests
{
    private IStateObserver _observer;

    [SetUp]
    public void Setup()
    {
        _observer = new StateObserver(new EventAggregator());
    }



    [Test]
    public void GetCurrentState_Initially_ReturnsDefaultValue()
    {
        // Act
        var state = _observer.GetCurrentState<StringStateChangedEvent, string>();

        // Assert
        Assert.That(state, Is.Null, "Initial state should be null for a reference type like string.");
    }

    [Test]
    public void SetNewState_UpdatesAndRetrievesCorrectState()
    {
        // Arrange
        const string expectedState = "Draft";

        // Act
        _observer.Dispatch<StringStateChangedEvent, string>(expectedState);
        var currentState = _observer.GetCurrentState<StringStateChangedEvent, string>();

        // Assert
        Assert.That(currentState, Is.EqualTo(expectedState), "GetCurrentState should return the last set state.");
    }

    [Test]
    public void SetNewState_DoesNotAffectOtherEventTypes()
    {
        // Arrange
        const string stringState = "Active";
        const int intState = 42;

        // Act: Set state for two different events
        _observer.Dispatch<StringStateChangedEvent, string>(stringState);
        _observer.Dispatch<IntStateChangedEvent, int>(intState);

        // Assert: Check that the string state is not affected by the int state
        var currentStringState = _observer.GetCurrentState<StringStateChangedEvent, string>();
        Assert.That(currentStringState, Is.EqualTo(stringState),
            "State for one event type was incorrectly overwritten by another.");
    }

    [Test]
    public void SubscribeAndSetNewState_PublishesEventToSubscriber()
    {
        // Arrange
        string receivedState = null!;
        const string newState = "Published";

        // Action to be executed on event publish
        void SubscriberAction(string state) => receivedState = state;

        // Subscribe
        _observer.Subscribe<StringStateChangedEvent, string>(SubscriberAction);

        // Act
        _observer.Dispatch<StringStateChangedEvent, string>(newState);

        // Assert
        Assert.That(receivedState, Is.EqualTo(newState), "The subscriber did not receive the published state.");
    }

    [Test]
    public void MultipleSubscribers_AllReceiveEvent()
    {
        // Arrange
        int callCount1 = 0;
        int callCount2 = 0;
        const string newState = "Finalized";

        // Subscribe two actions
        _observer.Subscribe<StringStateChangedEvent, string>(_ => callCount1++);
        _observer.Subscribe<StringStateChangedEvent, string>(_ => callCount2++);

        // Act
        _observer.Dispatch<StringStateChangedEvent, string>(newState);

        // Assert
        Assert.That(callCount1, Is.EqualTo(1), "First subscriber was not called.");
        Assert.That(callCount2, Is.EqualTo(1), "Second subscriber was not called.");
    }
}