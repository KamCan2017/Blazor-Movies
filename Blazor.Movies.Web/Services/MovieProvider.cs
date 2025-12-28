using System.Text.Json;
using Blazor.Movies.Web.Models;

namespace Blazor.Movies.Web.Services;

public interface IMovieProvider
{
    Task<Movie[]> LoadMovies(string filePath);
}



public class MovieProvider(ILogger<MovieProvider> logger) : IMovieProvider
{
    private readonly ILogger<MovieProvider> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    public async Task<Movie[]> LoadMovies(string filePath)
    {
        try
        {
            _logger.LogInformation("Loading movies....");
            if (string.IsNullOrEmpty(filePath)) throw new ArgumentNullException(nameof(filePath));
            await using var payload = File.OpenRead(filePath);
            var movies = await JsonSerializer.DeserializeAsync<Movie[]>(payload,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true },
                new CancellationTokenSource(TimeSpan.FromSeconds(5)).Token) ?? [];
            var validMovies = new List<Movie>();
            foreach (var movie in movies)
            {
                var result = movie.Validate();
                if (!result.isValid)
                {
                    _logger.LogError("Load for a movie failed: {Message}", result.errorMessage);
                }
                else
                {
                    validMovies.Add(movie);
                }
            }

            return validMovies.ToArray();
        }
        catch (Exception exception)
        {
            _logger.LogError("Load Movie failed: {Message}", exception.InnerException?.Message ?? exception.Message);
            return [];
        }
        finally
        {
            _logger.LogInformation("Loading movies is done.");
        }
    }


}