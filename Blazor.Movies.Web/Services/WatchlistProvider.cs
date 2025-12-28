
namespace Blazor.Movies.Web.Services;

public interface IWatchlistProvider
{
    void AddToWatchlist(int movieId);
    void RemoveFromWatchlist(int movieId);
    bool IsInWatchlist(int movieId);
    
    int[] GetWatchlistIds();
}
public class WatchlistProvider: IWatchlistProvider
{
    private static readonly IList<int> Watchlist = [1,2,4,8,12]; //Default watchlist
    private static readonly Lock LockObj = new();
    
    public void AddToWatchlist(int movieId)
    {
        using (LockObj.EnterScope())
        {
            if (Watchlist.Contains(movieId)) return;
            Watchlist.Add(movieId);
        }
    }

    public void RemoveFromWatchlist(int movieId)
    {
        using (LockObj.EnterScope())
        {
           Watchlist.Remove(movieId);
        }
        
    }

    public bool IsInWatchlist(int movieId)
    {
        using (LockObj.EnterScope())
        {
            return Watchlist.Contains(movieId);
        }
    }

    public int[] GetWatchlistIds()
    {
        using (LockObj.EnterScope())
        {
            return Watchlist.ToArray();
        }
    }
}