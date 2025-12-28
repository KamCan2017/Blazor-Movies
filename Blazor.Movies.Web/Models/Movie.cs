using Blazor.Movies.Web.Validators;
using FluentValidation.Results;

namespace Blazor.Movies.Web.Models;

public class Movie
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Image { get; set; } = string.Empty;
    public string Genre { get; set; } = string.Empty;
    public double Rating { get; set; }
    
    /// <summary>
    /// Used for the UI toggle event
    /// </summary>
    public bool IsInWatchList { get; set; }
}

public static class MovieExtension
{
    public static ValidationResult GetValidationResult(this Movie movie)
    {
        return new MovieValidator().Validate(movie);
    }
    
    public static (bool isValid, string errorMessage) Validate(this Movie movie)
    {
        var result = new MovieValidator().Validate(movie);
        return (result.IsValid, result.ToString());
    }
}