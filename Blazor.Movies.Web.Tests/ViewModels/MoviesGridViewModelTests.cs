using Blazor.Movies.Web.Models;
using Blazor.Movies.Web.Services;
using Blazor.Movies.Web.StateObservers;
using Blazor.Movies.Web.ViewModels;
using Moq;

namespace Blazor.Movies.Web.Tests.ViewModels;


[TestFixture]
public class MoviesGridViewModelTests
{
    private IStateObserver _stateObserver;
    private Mock<IMovieProvider> _movieMock;
    private Mock<IWatchlistProvider> _watchListProviderMock;
    private MoviesGridViewModel _viewModel;

    private readonly Movie[] _testMovies =
    [
       
        new(){Id = 1, Title = "Spectre", Rating = 8, Genre = MovieGenre.Action},
        new(){Id = 2, Title="The Piano", Rating = 5, Genre = MovieGenre.Drama,},
        new(){Id = 3, Title="Airplane", Rating = 4, Genre = MovieGenre.Comedy,},
        new(){Id = 4, Title="Avatar", Rating = 9, Genre = MovieGenre.Action}
    ];

    [SetUp]
    public void Setup()
    {
        // 1. Initialize dependencies
        _stateObserver = new StateObserver(new EventAggregator());
        _watchListProviderMock =  new Mock<IWatchlistProvider>();
        _movieMock = new Mock<IMovieProvider>();
        
        // 2. Mock: Setup MovieProvider to return TestMovies when LoadMovies is called
        _movieMock.Setup(p => p.LoadMovies(It.IsAny<string>()))
                  .ReturnsAsync(_testMovies);
        
        // 3. Instantiate the ViewModel (it subscribes to MoviesChanged in the constructor)
        _viewModel = new MoviesGridViewModel(_movieMock.Object, _stateObserver,  _watchListProviderMock.Object);
    }
    
    [Test]
    public void Constructor_SubscribesToMoviesChanged_AndInitializesCollections()
    {
        // Arrange: Initial state in observer is null or empty array.
        // Act: Simulate external publishing of movie state after VM creation
        _stateObserver.Dispatch<MoviesChanged, Movie[]>(_testMovies);

        // Assert: Subscription was successful, LoadedMovies is updated, and FilteredMovies is calculated
        Assert.That(_viewModel.LoadedMovies, Is.EquivalentTo(_testMovies), "LoadedMovies should be updated via subscription.");
        Assert.That(_viewModel.FilteredMovies.Count(), Is.EqualTo(4), "FilteredMovies should contain all loaded movies by default.");
    }

    // --- LoadMovies Tests ---
    
    [Test]
    public async Task LoadMovies_WhenNotLoaded_LoadsFromProviderAndSetsState()
    {
        // Arrange: Ensure MoviesLoaded is false (default unless set otherwise)
        _stateObserver.Dispatch<MoviesLoaded, bool>(false);
        _viewModel.LoadedMovies = [];

        // Act
        await _viewModel.LoadMovies();

        // Assert 1: Verify the movie provider mock was called once
        _movieMock.Verify(p => p.LoadMovies(MovieFilePath.GetFilePath()), Times.Once());
        
        // Assert 2: LoadedMovies is updated
        Assert.That(_viewModel.LoadedMovies, Is.EquivalentTo(_testMovies));
        
        // Assert 3: StateObserver flag is set
        Assert.That(_stateObserver.GetCurrentState<MoviesLoaded, bool>(), Is.True);
    }

    [Test]
    public async Task LoadMovies_WhenAlreadyLoaded_RecalculatesFilteringWithoutReloading()
    {
        // Arrange: Set MoviesLoaded to true and set the initial state in the observer
        _stateObserver.Dispatch<MoviesLoaded, bool>(true);
        _stateObserver.Dispatch<MoviesChanged, Movie[]>(_testMovies);
        
        // Also set an active filter
        _viewModel.SearchTerm = "Pia"; // Movie B (The Piano)
        _viewModel.SelectedGenre = MovieGenre.Drama;
        
        // Act
        await _viewModel.LoadMovies();

        // Assert 1: Verify the movie provider mock was NEVER called
        _movieMock.Verify(p => p.LoadMovies(It.IsAny<string>()), Times.Never());

        // Assert 2: Filtering was successfully applied using the current state
        Assert.That(_viewModel.FilteredMovies.Count(), Is.EqualTo(1));
        Assert.That(_viewModel.FilteredMovies.First().Title, Is.EqualTo("The Piano"));
    }

    // --- Filtering Tests ---

    [Test]
    public void HandleFiltering_NoFilters_ReturnsAllMovies()
    {
        // Arrange: Set LoadedMovies (no need to rely on the constructor subscription here)
        _viewModel.LoadedMovies = _testMovies;
        _viewModel.SearchTerm = string.Empty;
        _viewModel.SelectedGenre = MovieGenre.AllGenres;
        _viewModel.SelectedRating = MovieRating.All;

        // Act
        _viewModel.HandleFiltering();

        // Assert
        Assert.That(_viewModel.FilteredMovies.Count(), Is.EqualTo(4));
    }

    [Test]
    public void HandleFiltering_FiltersByTitle()
    {
        // Arrange
        _viewModel.LoadedMovies = _testMovies;
        _viewModel.SearchTerm = "AIR"; // Case insensitive match for "Airplane"

        // Act
        _viewModel.HandleFiltering();

        // Assert
        Assert.That(_viewModel.FilteredMovies.Count(), Is.EqualTo(1));
        Assert.That(_viewModel.FilteredMovies.First().Title, Is.EqualTo("Airplane"));
    }
    
    [Test]
    public void HandleFiltering_FiltersByGenre()
    {
        // Arrange
        _viewModel.LoadedMovies = _testMovies;
        _viewModel.SelectedGenre = MovieGenre.Action;

        // Act
        _viewModel.HandleFiltering();

        // Assert
        Assert.That(_viewModel.FilteredMovies.Count(), Is.EqualTo(2));
        Assert.That(_viewModel.FilteredMovies.Select(m => m.Id), Contains.Item(1));
        Assert.That(_viewModel.FilteredMovies.Select(m => m.Id), Contains.Item(4));
    }
    
    [Test]
    public void HandleFiltering_FiltersByRating_Good()
    {
        // Arrange: Rating >= 8 (Spectre=9, Avatar=9)
        _viewModel.LoadedMovies = _testMovies;
        _viewModel.SelectedRating = MovieRating.Good;

        // Act
        _viewModel.HandleFiltering();

        // Assert
        Assert.That(_viewModel.FilteredMovies.Count(), Is.EqualTo(2));
        Assert.That(_viewModel.FilteredMovies.Select(m => m.Title), Contains.Item("Spectre"));
        Assert.That(_viewModel.FilteredMovies.Select(m => m.Title), Contains.Item("Avatar"));
    }
    
    [Test]
    public void HandleFiltering_FiltersByRating_Ok()
    {
        // Arrange: Rating >= 5 and < 8 (The Piano=6)
        _viewModel.LoadedMovies = _testMovies;
        _viewModel.SelectedRating = MovieRating.Ok;

        // Act
        _viewModel.HandleFiltering();

        // Assert
        Assert.That(_viewModel.FilteredMovies.Count(), Is.EqualTo(1));
        Assert.That(_viewModel.FilteredMovies.Select(m => m.Title), Contains.Item("The Piano"));
    }
    
    [Test]
    public void HandleFiltering_FiltersByRating_Bad()
    {
        // Arrange: Rating < 5 (Airplane=3)
        _viewModel.LoadedMovies = _testMovies;
        _viewModel.SelectedRating = MovieRating.Bad;

        // Act
        _viewModel.HandleFiltering();

        // Assert
        Assert.That(_viewModel.FilteredMovies.Count(), Is.EqualTo(1));
        Assert.That(_viewModel.FilteredMovies.First().Title, Is.EqualTo("Airplane"));
    }

    [Test]
    public void HandleFiltering_FiltersByMultipleCriteria()
    {
        // Arrange: Action, Rating Good (>= 8)
        // Expected: Spectre (Action, 9)
        _viewModel.LoadedMovies = _testMovies;
        _viewModel.SelectedGenre = MovieGenre.Action;
        _viewModel.SelectedRating = MovieRating.Good;
        _viewModel.SearchTerm = "S"; // Match Spectre

        // Act
        _viewModel.HandleFiltering();

        // Assert
        Assert.That(_viewModel.FilteredMovies.Count(), Is.EqualTo(1));
        Assert.That(_viewModel.FilteredMovies.First().Title, Is.EqualTo("Spectre"));
    }
}
