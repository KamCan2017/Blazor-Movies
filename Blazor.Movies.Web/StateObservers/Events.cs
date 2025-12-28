using Blazor.Movies.Web.Models;

namespace Blazor.Movies.Web.StateObservers;

/// <summary>
/// Changed event after setting loaded movies.
/// </summary>
public class MoviesChanged: PubSubEvent<Movie[]>;

/// <summary>
/// Changed event after loading movies.
/// </summary>
public class MoviesLoaded: PubSubEvent<bool>;

/// <summary>
/// Changed event after removing or adding a movie to the watchlist.
/// </summary>
public class WatchListChanged: PubSubEvent<bool>;
