using Blazor.Movies.Web.Models;
using Blazor.Movies.Web.Services;
using Blazor.Movies.Web.StateObservers;
using Blazor.Movies.Web.ViewModels;
using Moq;

namespace Blazor.Movies.Web.Tests.ViewModels;

[TestFixture]
public class WatchListViewModelTests
{
    private IStateObserver _stateObserver;
    private Mock<IWatchlistProvider> _watchlistMock; // Using Moq
    private Mock<IMovieProvider> _movieMock; // Using Moq
    private WatchListViewModel _viewModel;

    private readonly Movie[] _mockInitialMovies =
    [
        new(){Id = 101, Title = "Movie A", Rating = 8},
        new(){Id = 102, Title = "Movie B", Rating = 8.5},
        new(){Id = 103, Title = "Movie C", Rating = 6.5},
        new(){Id = 104, Title = "Movie D", Rating = 4.5}
    ];

    private readonly Movie[] _mockLoadedMovies =
    [
        new(){Id = 201, Title = "New Movie E", Rating = 8},
        new(){Id = 202, Title = "New Movie F", Rating = 7.5}
    ];

    [SetUp]
    public void Setup()
    {
        // 1. Initialize dependencies
        _stateObserver = new StateObserver(new EventAggregator());
        _watchlistMock = new Mock<IWatchlistProvider>();
        _movieMock = new Mock<IMovieProvider>();
        
        // 2. Mock: Pre-configure watchlist for initial state (Movie C, Id 103)
        // Setup IsInWatchlist to return true only for ID 103 initially.
        _watchlistMock.Setup(p => p.IsInWatchlist(It.IsAny<int>()))
                      .Returns((int id) => id == 103);
        
        // 3. Mock: Setup MovieProvider to return MockLoadedMovies when LoadMovies is called
        _movieMock.Setup(p => p.LoadMovies(It.IsAny<string>()))
                  .ReturnsAsync(_mockLoadedMovies);
        
        // 4. Set a dummy initial movie state in the observer before ViewModel creation
        _stateObserver.Dispatch<MoviesChanged, Movie[]>(_mockInitialMovies);
        
        // 5. Instantiate the ViewModel using Mock Objects
        _viewModel = new WatchListViewModel(_watchlistMock.Object, _movieMock.Object, _stateObserver);
    }

    [Test]
    public void Constructor_InitializesCollectionsCorrectly_BasedOnCurrentState()
    {
        // Arrange/Act done in Setup

        // Assert 1: LoadedMovies reflects the state BEFORE ViewModel was created
        Assert.That(_viewModel.LoadedMovies, Is.EquivalentTo(_mockInitialMovies), 
            "LoadedMovies should be initialized from IStateObserver.GetCurrentState.");
    }

    [Test]
    public void UpdateMoviesCollection_WhenMoviesChangedEventFires_UpdatesBothCollections()
    {
        // Arrange
        // Update mock setup to reflect that MockLoadedMovies' first item (201) is now in the watchlist
        _watchlistMock.Setup(p => p.IsInWatchlist(It.IsAny<int>()))
                      .Returns((int id) => id == 201); // Only Movie E (201) is now in watchlist

        // Act: Simulate an external entity setting a new movie state
        _stateObserver.Dispatch<MoviesChanged, Movie[]>(_mockLoadedMovies);

        // Assert 1: LoadedMovies is updated
        Assert.That(_viewModel.LoadedMovies, Is.EquivalentTo(_mockLoadedMovies));
    }

    
    [Test]
    public void WatchListChangedEvent_RecalculatesWatchList_BasedOnNewProviderState()
    {
        // Arrange: ViewModel is initialized with MockInitialMovies. Movie C (103) is on watchlist.
        
        // Act 1: Update mock setup to reflect that the watchlist is now empty (no ID returns true)
        _watchlistMock.Setup(p => p.IsInWatchlist(It.IsAny<int>())).Returns(false);

        // Act 2: Fire the WatchListChanged event via the StateObserver (which the ViewModel subscribes to)
        _stateObserver.Dispatch<WatchListChanged, bool>(true);

        // Assert: Watchlist should now be empty
        Assert.That(_viewModel.MovieWatchList, Is.Empty, "Watchlist should be empty after removal and event signal.");
    }
    
    [Test]
    public async Task LoadWatchList_WhenMoviesNotLoaded_LoadsAndSetsState()
    {
        // Arrange: Reset the observer state to "not loaded"
        _stateObserver.Dispatch<MoviesLoaded, bool>(false);
        _stateObserver.Dispatch<MoviesChanged, Movie[]>([]);
        
        // Act
        await _viewModel.LoadWatchList();

        // Assert 1: Verify the movie provider mock was called once
        _movieMock.Verify(p => p.LoadMovies(MovieFilePath.GetFilePath()), Times.Once());
        
        // Assert 2: ViewModel's collections are updated (via SetNewState -> Subscription)
        Assert.That(_viewModel.LoadedMovies, Is.EquivalentTo(_mockLoadedMovies), 
            "LoadedMovies should be updated with data from IMovieProvider.");
        
        // Assert 3: StateObserver flag is set
        var isLoaded = _stateObserver.GetCurrentState<MoviesLoaded, bool>();
        Assert.That(isLoaded, Is.True, "The MoviesLoaded state flag should be set to true.");
    }
    
    [Test]
    public async Task LoadWatchList_WhenMoviesAlreadyLoaded_DoesNotReload()
    {
        // Arrange: MoviesLoaded is true 
        _stateObserver.Dispatch<MoviesLoaded, bool>(true);
        _viewModel.LoadedMovies = null!;

        // Act
        await _viewModel.LoadWatchList();

        // Assert 1: Verify the movie provider mock was NEVER called
        _movieMock.Verify(p => p.LoadMovies(It.IsAny<string>()), Times.Never(),
                          "IMovieProvider.LoadMovies should not be called if MoviesLoaded flag is true.");
        
        // Assert 2: LoadedMovies should remain null because the reload was skipped
        Assert.That(_viewModel.LoadedMovies, Is.Null, "Movie list should not be reloaded if MoviesLoaded flag is true.");
    }
    
   
}