using Blazor.Movies.Web.Models;
using Blazor.Movies.Web.Services;
using Blazor.Movies.Web.StateObservers;

namespace Blazor.Movies.Web.ViewModels.Scenario2;

public class WatchListViewModel2
{
    private readonly IMovieProvider _movieProvider;
    private readonly IStateManager _stateManager;
    private int[] _watchListIds;
    public WatchListViewModel2(IWatchlistProvider watchlistProvider, IMovieProvider movieProvider,
        IStateManager stateManager)
    {
        var localWatchListProvider = watchlistProvider ?? throw new ArgumentNullException(nameof(watchlistProvider));
        _movieProvider = movieProvider ?? throw new ArgumentNullException(nameof(movieProvider));
        _stateManager= stateManager ?? throw new ArgumentNullException(nameof(stateManager));
        _stateManager.Subscribe(StateKeys.Watchlist, state =>
        {
            _watchListIds = localWatchListProvider.GetWatchlistIds();
            MovieWatchList = LoadedMovies.Where(movie => _watchListIds.Contains(movie.Id)).ToArray();
        });
        _stateManager.Subscribe(StateKeys.AllMovies, state =>
        {
            UpdateMoviesCollection(state as Movie[] ?? []);
        });
       
        _watchListIds = localWatchListProvider.GetWatchlistIds();
        UpdateMoviesCollection(_stateManager.GetCurrentState(StateKeys.AllMovies) as Movie[] ?? []);
        
    }
    public IEnumerable<Movie> MovieWatchList { get; private set; } = null!;

    public Movie[] LoadedMovies { get; set; } = null!;

    public async Task LoadWatchList()
    {
        var moviesAlreadyLoaded = (bool)(_stateManager.GetCurrentState(StateKeys.MoviesLoaded) ?? false);
        if (!moviesAlreadyLoaded)
        {
            _stateManager.UpdateState(StateKeys.AllMovies,  await _movieProvider.LoadMovies(MovieFilePath.GetFilePath()));
            _stateManager.UpdateState(StateKeys.MoviesLoaded, true);
        }
    }

    private void UpdateMoviesCollection(Movie[] movies)
    {
        ArgumentNullException.ThrowIfNull(movies);
        LoadedMovies = movies;
        MovieWatchList = LoadedMovies.Where(movie =>
        {
            var result = _watchListIds.Contains(movie.Id);
            movie.IsInWatchList = result;
            return result;
        }).ToArray();
    }
}