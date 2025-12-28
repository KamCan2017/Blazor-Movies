using Blazor.Movies.Web.Models;
using FluentValidation;

namespace Blazor.Movies.Web.Validators;

/// <summary>
/// Fluent Validation implementation for the Movie class.
/// </summary>
public class MovieValidator : AbstractValidator<Movie>
{
    public MovieValidator()
    {
        // --- ID Validation ---
        RuleFor(movie => movie.Id)
            .GreaterThan(0).WithMessage("Movie ID must be a positive integer.");

        // --- Title Validation ---
        RuleFor(movie => movie.Title)
            .NotEmpty().WithMessage("Title is required.")
            .MaximumLength(250).WithMessage("Title cannot exceed 250 characters.")
            .MinimumLength(2).WithMessage("Title must be at least 2 characters long.");

        // --- Image Validation (Simple check for non-empty) ---
        // In a real application, you might add a custom RuleFor URL/Path validity.
        RuleFor(movie => movie.Image)
            .NotEmpty().WithMessage("Image path/URL is required.");

        // --- Genre Validation ---
        RuleFor(movie => movie.Genre)
            .NotEmpty().WithMessage("Genre is required.")
            .Must(BeAValidGenre).WithMessage("Genre contains invalid characters or format.");
        
        // --- Rating Validation ---
        RuleFor(movie => movie.Rating)
            .InclusiveBetween(1.0, 10.0).WithMessage("Rating must be between 0.0 and 10.0 (inclusive).");
    }

    /// <summary>
    /// Custom rule to check if the genre string contains only letters and spaces.
    /// </summary>
    /// <param name="genre">The genre string to validate.</param>
    /// <returns>True if the genre is valid, false otherwise.</returns>
    private static bool BeAValidGenre(string genre)
    {
        return !string.IsNullOrWhiteSpace(genre) &&
               // Allows letters, spaces, and hyphens (common in genres like "Sci-Fi")
               genre.All(c => char.IsLetter(c) || char.IsWhiteSpace(c));
    }
}
