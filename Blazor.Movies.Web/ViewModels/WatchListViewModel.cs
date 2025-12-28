using Blazor.Movies.Web.Models;
using Blazor.Movies.Web.Services;
using Blazor.Movies.Web.StateObservers;

namespace Blazor.Movies.Web.ViewModels;

public class WatchListViewModel
{
    private readonly IMovieProvider _movieProvider;
    private readonly IStateObserver _stateObserver;
    private int[] _watchListIds;
    public WatchListViewModel(IWatchlistProvider watchlistProvider, IMovieProvider movieProvider, IStateObserver stateObserver)
    {
        var localWatchListProvider = watchlistProvider ?? throw new ArgumentNullException(nameof(watchlistProvider));
        _movieProvider = movieProvider ?? throw new ArgumentNullException(nameof(movieProvider));
        _stateObserver = stateObserver ?? throw new ArgumentNullException(nameof(stateObserver));
        _stateObserver.Subscribe<MoviesChanged,Movie[]>(UpdateMoviesCollection);
        _stateObserver.Subscribe<WatchListChanged,bool>(_ =>
        {
            _watchListIds = localWatchListProvider.GetWatchlistIds();
            MovieWatchList = LoadedMovies.Where(movie => _watchListIds.Contains(movie.Id)).ToArray();
        });
        _watchListIds = localWatchListProvider.GetWatchlistIds();
        UpdateMoviesCollection(_stateObserver.GetCurrentState<MoviesChanged, Movie[]>() ?? []);
        
    }
    public IEnumerable<Movie> MovieWatchList { get; private set; } = null!;

    public Movie[] LoadedMovies { get; set; } = null!;

    public async Task LoadWatchList()
    {
        var moviesAlreadyLoaded = _stateObserver.GetCurrentState<MoviesLoaded,bool>();
        if (!moviesAlreadyLoaded)
        {
            _stateObserver.Dispatch<MoviesChanged,Movie[]>(await _movieProvider.LoadMovies(MovieFilePath.GetFilePath()));
            _stateObserver.Dispatch<MoviesLoaded, bool>(true);
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