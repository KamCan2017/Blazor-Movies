using Blazor.Movies.Web.StateObservers;

namespace Blazor.Movies.Web.Tests.StateObservers
{
    public record TestModel(int Id, string Name);
   
    [TestFixture]
    public class StateManagerTests
    {
        private StateManager _stateManager;

        [SetUp]
        public void SetUp()
        {
            // Initialize the StateManager before each test
            _stateManager = new StateManager();
        }

        [TearDown]
        public void TearDown()
        {
            _stateManager.Dispose();
        }
       

        [Test]
        public void Subscribe_ToExistingState_AddsHandlerToExistingSubscriber()
        {
            // Arrange
            var stateName = "user_id";
            var initialValue = 100;
            var stateA = 0;
            var stateB = 0;
            
            // 1. Initial subscription and update to create the subscriber object
            _stateManager.UpdateState(stateName, initialValue);
            
            // 2. Handler A (already subscribed indirectly via initial update)
            StateSubscriber.StateEventHandler handlerA = (state) => { stateA = (int)state; };
            _stateManager.Subscribe(stateName, handlerA);

            // 3. Handler B (second handler being added to the existing state)
            StateSubscriber.StateEventHandler handlerB = (state) => { stateB = (int)state; };
            _stateManager.Subscribe(stateName, handlerB);
            
            // Act
            // Updating the state should fire both Handler A and Handler B
            _stateManager.UpdateState(stateName, 200);

            // Assert
            Assert.That(stateA, Is.EqualTo(200), "The existing subscriber should have invoked both handlers.");
            Assert.That(stateB, Is.EqualTo(200), "The existing subscriber should have invoked both handlers.");
        }
        
        [Test]
        public void Subscribe_ToExistingStateManyTime_AddsHandlerOneTimeToExistingSubscriber()
        {
            // Arrange
            var stateName = "user_id";
            var initialValue = 100;
            var stateA = 0;
            var count = 0;
            
            // 1. Initial subscription and update to create the subscriber object
            _stateManager.UpdateState(stateName, initialValue);
            
            // 2. Handler A (already subscribed indirectly via initial update)
            StateSubscriber.StateEventHandler handlerA = (state) => {
                stateA = (int)state;
                count++;
            };
            _stateManager.Subscribe(stateName, handlerA);
            _stateManager.Subscribe(stateName, handlerA);
            
            // Act
            // Updating the state should fire both Handler A and Handler B
            _stateManager.UpdateState(stateName, 200);

            // Assert
            Assert.That(stateA, Is.EqualTo(200), "The existing subscriber should have invoked one time the handler.");
            Assert.That(count, Is.EqualTo(1), "The existing subscriber should have invoked one time the handler.");
        }
        
        [TestCase(""), TestCase(null)]
        public void Subscribe_WithInvalidStateKey_ThrowsException(string? stateKey)
        {
            Assert.Throws<ArgumentNullException>(() => _stateManager.Subscribe(stateKey!, (_) => { }));
        }
       
        
        [Test]
        public void Subscribe_WithNullHandler_ThrowsException()
        {
            StateSubscriber.StateEventHandler stateHandler = null!;
            Assert.Throws<ArgumentNullException>(() => _stateManager.Subscribe("test", stateHandler));
        }
        
         
        [Test]
        public void Unsubscribe_WithNullHandler_ThrowsException()
        {
            StateSubscriber.StateEventHandler stateHandler = null!;
            Assert.Throws<ArgumentNullException>(() => _stateManager.Unsubscribe("test", stateHandler));
        }
        
        [TestCase(""), TestCase(null)]
        public void Unsubscribe_WithInvalidStateKey_ThrowsException(string? stateKey)
        {
            Assert.Throws<ArgumentNullException>(() => _stateManager.Unsubscribe(stateKey!, (_) => { }));
        }
        
        
        [Test]
        public void Unsubscribe_ToExistingState_RemovesHandlerToExistingSubscriber()
        {
            // Arrange
            var stateName = "user_id";
            var initialValue = 0;
            var stateA = 100;
            
            // 1. Initial subscription and update to create the subscriber object
            _stateManager.UpdateState(stateName, initialValue);
          
            StateSubscriber.StateEventHandler handlerA = (state) => { stateA = (int)state; };
            _stateManager.Subscribe(stateName, handlerA);
            _stateManager.Unsubscribe(stateName, handlerA);
            
            // Act
            // Updating the state should not fire the Handler A
            _stateManager.UpdateState(stateName, 200);

            // Assert
            Assert.That(stateA, Is.EqualTo(100), "The existing subscriber should have invoked both handlers.");
        }
        
        [Test]
        public void UpdateState_NewState_CreatesSubscriberAndFiresEvent()
        {
            // Arrange
            var stateName = "status";
            var expectedValue = "online";
            var receivedValue = string.Empty;

            // Subscribe a handler before the state exists
            _stateManager.Subscribe(stateName, (state) => { receivedValue = (string)state; });

            // Act
            _stateManager.UpdateState(stateName, expectedValue);

            // Assert
            Assert.That(receivedValue, Is.EqualTo(expectedValue), "New state update failed to fire the subscribed event with the correct value.");
        }

        [Test]
        public void UpdateState_ExistingStateWithNewValue_UpdatesStateAndFiresEvent()
        {
            // Arrange
            var stateName = "volume";
            var initialValue = 50;
            var newValue = 75;
            var receivedValue = 0;
            var updateCount = 0;

            // 1. Initial update to create the state object
            _stateManager.UpdateState(stateName, initialValue);

            // 2. Subscribe a handler
            _stateManager.Subscribe(stateName, (state) => { 
                receivedValue = (int)state;
                updateCount++;
            });
            
            // Act
            _stateManager.UpdateState(stateName, newValue);

            // Assert
            Assert.That(receivedValue, Is.EqualTo(newValue), "Existing state update failed to update the state object.");
            Assert.That(updateCount, Is.EqualTo(1), "Existing state update should fire the event exactly once.");
        }

        [Test]
        public void UpdateState_ExistingStateWithSameValue_DoesNotFireEvent()
        {
            // Arrange
            var stateName = "is_loading";
            var model = new TestModel(1, "test");
            var copyModel = model;
            var updateCount = 0;

            // 1. Initial update and subscription
            _stateManager.UpdateState(stateName, model);
            _stateManager.Subscribe(stateName, _ => { updateCount++; });

            // Act
            // Attempt to update with the same value
            _stateManager.UpdateState(stateName, copyModel); 

            // Assert
            // The event handler was subscribed. The UpdateState call above should return early.
            Assert.That(updateCount, Is.EqualTo(0), "Updating with the same value should NOT fire the event.");
        }
        
        [Test]
        public void UpdateState_ChangingStateType_ThrowsException()
        {
            // Arrange
            var stateName = "is_loading";
            bool value = true;

            // 1. Initial update and subscription
            _stateManager.UpdateState(stateName, value);
            _stateManager.Subscribe(stateName, _ => {  });

            // Act
            // Attempt to update with ta new type
            
            Assert.Throws<Exception>(() =>_stateManager.UpdateState(stateName,"test")); 
        }
        
        [TestCase(""), TestCase(null)]
        public void UpdateState_InvalidStateKey_ThrowsException(string? stateKey)
        {
          
            // Act
            // Attempt to update with ta new type
            
            Assert.Throws<ArgumentNullException>(() =>_stateManager.UpdateState(stateKey!,"test")); 
        }

        [Test]
        public void UpdateState_CaseInsensitiveKeyMatching_WorksCorrectly()
        {
            // Arrange
            var stateName = "UserName"; // Mixed case
            var keyInManager = "username"; // Lower invariant key in manager
            var expectedValue = "Alice";
            var receivedValue = string.Empty;

            // 1. Subscribe using mixed case
            _stateManager.Subscribe(stateName, (state) => { receivedValue = (string)state; });

            // Act
            // 2. Update using lowercase (should match the subscribed key)
            _stateManager.UpdateState(keyInManager, expectedValue);

            // Assert
            Assert.That(receivedValue, Is.EqualTo(expectedValue), "State lookup is not case-insensitive.");
        }

        [TestCase(null),  TestCase("")]
        public void GetCurrenState_ByInvalidStateKey_ThrowsException(string? stateName)
        {
            Assert.Throws<ArgumentNullException>(() => _stateManager.GetCurrentState(stateName!));
        }
        
        [Test]
        public void GetCurrenState_ByNotRegisteredState_ReturnsNullReference()
        {
            //Arrange
            var stateName = "user_key";
            
            //Act
            var state = _stateManager.GetCurrentState(stateName);
            
            //Assert
            Assert.That(state, Is.Null);
        }
        
        [Test]
        public void GetCurrenState_ByRegisteredState_ReturnsState()
        {
            //Arrange
            var stateName = "user_key";
            var initialValue = "theme";
            _stateManager.UpdateState(stateName, initialValue);
            
            //Act
            var state = _stateManager.GetCurrentState(stateName);
            
            //Assert
            Assert.That(state, Is.EqualTo(initialValue));
        }
    }
}