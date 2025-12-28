using Blazor.Movies.Web.Models;
using Blazor.Movies.Web.Services;
using Blazor.Movies.Web.StateObservers;

namespace Blazor.Movies.Web.ViewModels;

public class MoviesGridViewModel
{
    private readonly IMovieProvider _movieProvider;
    private readonly IStateObserver _stateObserver;
    private readonly IWatchlistProvider _watchlistProvider;
    
    public MoviesGridViewModel(IMovieProvider movieProvider, IStateObserver stateObserver, IWatchlistProvider watchlistProvider)
    {
      _movieProvider = movieProvider ?? throw new ArgumentNullException(nameof(movieProvider));
      _watchlistProvider  = watchlistProvider ?? throw new ArgumentNullException(nameof(watchlistProvider));
      _stateObserver = stateObserver ?? throw new ArgumentNullException(nameof(stateObserver));
      _stateObserver.Subscribe<MoviesChanged,Movie[]>(UpdateInternalMovieCollection);
    }
    public IEnumerable<Movie> FilteredMovies { get; private set; } = [];
    public IEnumerable<Movie> LoadedMovies { get; set; } = [];

    public string SearchTerm { get; set; } = string.Empty;
    public string SelectedGenre { get; set; } = MovieGenre.AllGenres;
    public string SelectedRating { get; set; } = MovieRating.All;
    
    public async Task LoadMovies()
    {
        var moviesAlreadyLoaded = _stateObserver.GetCurrentState<MoviesLoaded,bool>();
        if (moviesAlreadyLoaded)
        {
           UpdateInternalMovieCollection(_stateObserver.GetCurrentState<MoviesChanged, Movie[]>() ?? []);
           return;
        }
        _stateObserver.Dispatch<MoviesChanged,Movie[]>(await _movieProvider.LoadMovies(MovieFilePath.GetFilePath()));
        _stateObserver.Dispatch<MoviesLoaded, bool>(true);
    }


    public void HandleFiltering()
    {
        FilteredMovies = LoadedMovies.Where(movie => MatchesTitle(movie, SearchTerm) && MatchesGenre(movie, SelectedGenre) && MatchesRating(movie, SelectedRating));
    }

    private void UpdateInternalMovieCollection(Movie[] movies)
    {
        ArgumentNullException.ThrowIfNull(movies);
        var watchlistIds = _watchlistProvider.GetWatchlistIds();
        foreach (var movie in movies)
        {
            movie.IsInWatchList = watchlistIds.Contains(movie.Id);
        }
        LoadedMovies = movies;
        HandleFiltering();
    }
    
    private static bool MatchesTitle(Movie movie, string searchTerm)
    {
        return string.IsNullOrEmpty(searchTerm) || movie.Title.Contains(searchTerm, StringComparison.InvariantCultureIgnoreCase);
    }

    private static bool MatchesGenre(Movie movie, string genre)
    {
        return string.Equals(genre.ToLowerInvariant(), MovieGenre.AllGenres.ToLowerInvariant(), StringComparison.InvariantCulture) || movie.Genre.Contains(genre, StringComparison.InvariantCultureIgnoreCase);
    }

    private static bool MatchesRating(Movie movie, string rating)
    {
        return rating switch
        {
            MovieRating.All => true,
            MovieRating.Good => movie.Rating >= 8,
            MovieRating.Bad => movie.Rating < 5,
            MovieRating.Ok => movie.Rating is >= 5 and < 8,
            _ => false
        };
    }

}