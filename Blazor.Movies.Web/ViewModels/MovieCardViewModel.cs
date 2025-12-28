using Blazor.Movies.Web.Models;
using Blazor.Movies.Web.Services;
using Blazor.Movies.Web.StateObservers;

namespace Blazor.Movies.Web.ViewModels;

public class MovieCardViewModel(IWatchlistProvider watchlistProvider, IStateObserver stateObserver)
{
    private readonly IWatchlistProvider _watchlistProvider = watchlistProvider ?? throw new ArgumentNullException(nameof(watchlistProvider));
  private readonly IStateObserver _stateObserver =
        stateObserver ?? throw new ArgumentNullException(nameof(stateObserver));

    public Movie Movie { get; set; } = new ();

 
    public bool ShowImage {get; private set;} = true;

    public void HandleErrorByLoadingImage()
    {
        // 1. Log the error (optional but recommended)
        Console.WriteLine($"Error loading image for movie: {Movie.Title}. Image path: images/{Movie.Image}");

        // 2. Set the state to hide the image and display the fallback content
        // This is the primary mechanism to handle the error visually.
        ShowImage = false;
    }

    public void ToggleWatchlist()
    {
        if(Movie.IsInWatchList) {_watchlistProvider.AddToWatchlist(Movie.Id);}
        else _watchlistProvider.RemoveFromWatchlist(Movie.Id);
        _stateObserver.Dispatch<WatchListChanged,bool>(true);
    }
    
    public static string GetRatingClass(double rating)
    {
        return rating switch
        {
            >= 8 => "rating-good",
            >= 5 and < 8 => "rating-ok",
            _ => "rating-bad"
        };
    }
}