using Blazor.Movies.Web.Models;
using Blazor.Movies.Web.Services;
using Blazor.Movies.Web.StateObservers;

namespace Blazor.Movies.Web.ViewModels.Scenario2;

public class MovieCardViewModel2(IWatchlistProvider watchlistProvider,
    IStateManager stateManager)
{
    private readonly IWatchlistProvider _watchlistProvider = watchlistProvider ?? throw new ArgumentNullException(nameof(watchlistProvider));
    private readonly IStateManager _stateManager =  stateManager ?? throw new ArgumentNullException(nameof(stateManager));

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
        if (_stateManager.GetCurrentState(StateKeys.Watchlist) is bool toggled)
            _stateManager.UpdateState(StateKeys.Watchlist, !toggled);
        else
            _stateManager.UpdateState(StateKeys.Watchlist, true);
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