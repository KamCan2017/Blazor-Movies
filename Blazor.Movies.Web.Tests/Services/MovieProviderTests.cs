using Blazor.Movies.Web.Models;
using Blazor.Movies.Web.Services;
using Microsoft.Extensions.Logging;
using Moq;

namespace Blazor.Movies.Web.Tests.Services
{
    [TestFixture]
    public class MovieProviderTests
    {
        private MovieProvider _movieProvider;
        private readonly Mock<ILogger<MovieProvider>> _loggerMock = new();
        
        [SetUp]
        public void SetUp()
        {
            _movieProvider = new MovieProvider(_loggerMock.Object);
        }
        
        [Test]
        public void Constructor_ThrowsArgumentNullException_OnNullDependencies()
        {
            // Test null watchlistProvider
            Assert.That(() => new MovieProvider(null!),
                Throws.ArgumentNullException.With.Property("ParamName").EqualTo("logger"));
        }

        [Test]
        public async Task LoadMovies_ReturnsMovieArray_WhenFileExistsAndIsValid()
        {
            // Arrange
            var expectedMovies = new[]
            {
                new Movie { Id = 1, Title = "The Matrix", Rating = 8, Genre = "Science Fiction", Image = "1.jpg"},
                new Movie { Id = 2, Title = "Inception", Rating = 6, Genre = "Thriller", Image = "2.jpg"}
            };

            var testFilePath = Path.Combine(Environment.CurrentDirectory, "movies.json");

            // Act
            var result = await _movieProvider.LoadMovies(testFilePath);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Length, Is.EqualTo(expectedMovies.Length));
            for (var i = 0; i < result.Length; i++)
            {
                Assert.That(result[i].Title.ToLowerInvariant(), Is.EqualTo(expectedMovies[i].Title.ToLowerInvariant()));
                Assert.That(result[i].Rating, Is.EqualTo(expectedMovies[i].Rating));
                Assert.That(result[i].Genre.ToLowerInvariant(), Is.EqualTo(expectedMovies[i].Genre.ToLowerInvariant()));
                Assert.That(result[i].Image.ToLowerInvariant(), Is.EqualTo(expectedMovies[i].Image.ToLowerInvariant()));
            }

        }

        [Test]
        public async Task LoadMovies_ReturnsEmptyArray_WhenFileDoesNotExist()
        {
            // Arrange
            var filePath = Path.Combine(Environment.CurrentDirectory, "does-not-exist.json");

            // Act
            var result = await _movieProvider.LoadMovies(filePath);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Length, Is.EqualTo(0));
        }

        [Test]
        public async Task LoadMovies_ReturnsEmptyArray_WhenFileIsEmpty()
        {
            // Arrange

            // Act
            var result = await _movieProvider.LoadMovies( Path.Combine(Environment.CurrentDirectory, "emptymovies.json"));

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Length, Is.EqualTo(0));
        }
        
        [Test]
        public async Task LoadMovies_ReturnsOnlyValidMoviesArray_WhenMovieDataIsInvalid()
        {
            // Arrange
            var filePath = Path.Combine(Environment.CurrentDirectory, "invalidmovies.json");

            // Act
            var result = await _movieProvider.LoadMovies(filePath);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Has.Length.EqualTo(1));
        }
    }
}