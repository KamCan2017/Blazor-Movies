namespace Blazor.Movies.Web.Models;

public static class MovieGenre
{
    public const string Action = "Action";
    public const string Comedy = "Comedy";
    public const string Drama = "Drama";
    public const string Thriller = "Thriller";
    public const string Fantasy = "Fantasy";
    public const string ScienceFiction = "Science Fiction";
    public const string AllGenres = "All Genres";
}

public static class MovieRating
{
    public const string All = "All";
    public const string Good = "Good";
    public const string Bad = "Bad";
    public const string Ok = "Ok";
}

public static class MovieFilePath
{
    private const string RelativeFilePath = "movies.json";
    public static string GetFilePath()
    {
        // The WebRootPath is the physical path to your wwwroot folder.
        var wwwRootPath = Environment.CurrentDirectory;

        // Combine the wwwroot path with the relative file path to get the full physical path.
        return Path.Combine(wwwRootPath, RelativeFilePath);
    }
}

public static class StateKeys
{
  public const string AllMovies = "allmovies";
  public const string Watchlist = "watchlist";
  public const string MoviesLoaded =  "moviesloaded";
}