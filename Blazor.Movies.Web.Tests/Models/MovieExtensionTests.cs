using Blazor.Movies.Web.Models;

namespace Blazor.Movies.Web.Tests.Models;

[TestFixture]
public class MovieExtensionTests
{
    [Test]
    public void GetValidationResult_WithValidMovie_ReturnsValidResult()
    {
        // Arrange
        var movie = new Movie
        {
            Id = 1,
            Title = "Cosmic Legion",
            Rating = 8.5,
            Genre = "action",
            Image = "2.png"
        };

        // Act
        var result = movie.GetValidationResult();

        // Assert
        Assert.That(result.IsValid, Is.True);
    }

    [Test]
    public void GetValidationResult_WithInvalidMovie_ReturnsInvalidResultWithErrors()
    {
        // Arrange
        var movie = new Movie { Id = 0, Title = "", Rating = 11, Genre = "action", Image = "2.png"};

        // Act
        var result = movie.GetValidationResult();

        // Assert
        Assert.That(result.IsValid, Is.False);
        Assert.That(result.Errors, Has.Count.EqualTo(4));
    }

    [Test]
    public void Validate_WithValidMovie_ReturnsTrueAndEmptyErrorMessage()
    {
        // Arrange
        var movie = new Movie { Id = 1, Title = "Stargazer Protocol", Rating = 7, Genre = "action", Image = "3.png"};

        // Act
        var (isValid, errorMessage) = movie.Validate();

        // Assert
        Assert.That(isValid, Is.True);
        Assert.That(errorMessage, Is.Empty);
    }

    [TestCaseSource(nameof(InvalidMovies))]
    public void Validate_WithInvalidMovie_ReturnsFalseAndFormattedErrorMessage(Movie movie, string expectedErrorMessage)
    {
        
        // Act
        var (isValid, errorMessage) = movie.Validate();

        Assert.Multiple(() =>
        {
            // Assert
            Assert.That(isValid, Is.False);
            Assert.That(errorMessage, Does.Contain(expectedErrorMessage));
        });
    }

    public static object[] InvalidMovies =
    [
        new object[] { new Movie { Id = 1, Title = "", Image = "1.png", Genre = "drama", Rating = 8.5}, "Title is required"},
        new object[] { new Movie { Id = -1, Title = "vision", Image = "1.png", Genre = "drama", Rating = 8.5}, "Movie ID must be a positive integer"},
        new object[] { new Movie { Id = 1, Title = "vision", Image = "", Genre = "drama", Rating = 8.5}, "Image path/URL is required"},
        new object[] { new Movie { Id = 1, Title = "vision", Image = "1.png", Genre = "", Rating = 8.5}, "Genre is required"},
        new object[] { new Movie { Id = 1, Title = "vision", Image = "1.png", Genre = "action", Rating = 0.5}, "Rating must be between 0.0 and 10.0 (inclusive)."},
        new object[] { new Movie { Id = 1, Title = "vision", Image = "1.png", Genre = "action", Rating = 15.5}, "Rating must be between 0.0 and 10.0 (inclusive)."}
       
    ];
}