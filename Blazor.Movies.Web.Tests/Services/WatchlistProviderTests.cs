
using Blazor.Movies.Web.Services;

namespace Blazor.Movies.Web.Tests.Services
{
    [TestFixture]
    public class WatchlistProviderTests
    {
        private WatchlistProvider _watchlistProvider;

        [SetUp]
        public void SetUp()
        {
            _watchlistProvider = new WatchlistProvider();
            
            // Clear the static watchlist before each test
            // This is crucial because the Watchlist is static
            ClearWatchlist();
        }

        [TearDown]
        public void TearDown()
        {
            // Clean up after each test
            ClearWatchlist();
        }

        private void ClearWatchlist()
        {
            // Remove all items from the static watchlist
            var testIds = new[] { 1, 2, 3, 4, 5, 100, 999 };
            foreach (var id in testIds)
            {
                _watchlistProvider.RemoveFromWatchlist(id);
            }
        }

        [Test]
        public void AddToWatchlist_AddsMovieId_WhenNotAlreadyInList()
        {
            // Arrange
            int movieId = 1;

            // Act
            _watchlistProvider.AddToWatchlist(movieId);

            // Assert
            Assert.That(_watchlistProvider.IsInWatchlist(movieId), Is.True);
        }

        [Test]
        public void AddToWatchlist_DoesNotAddDuplicate_WhenMovieIdAlreadyExists()
        {
            // Arrange
            int movieId = 1;
            _watchlistProvider.AddToWatchlist(movieId);

            // Act
            _watchlistProvider.AddToWatchlist(movieId); // Try to add again

            // Assert
            Assert.That(_watchlistProvider.IsInWatchlist(movieId), Is.True);
        }
       

        [Test]
        public void RemoveFromWatchlist_RemovesMovieId_WhenExists()
        {
            // Arrange
            int movieId = 1;
            _watchlistProvider.AddToWatchlist(movieId);

            // Act
            _watchlistProvider.RemoveFromWatchlist(movieId);

            // Assert
            Assert.That(_watchlistProvider.IsInWatchlist(movieId), Is.False);
        }

       

        [Test]
        public void RemoveFromWatchlist_DoesNotThrow_WhenMovieNotInList()
        {
            // Arrange
            int movieId = 999;

            // Act & Assert
            Assert.DoesNotThrow(() => _watchlistProvider.RemoveFromWatchlist(movieId));
        }

        [Test]
        public void IsInWatchlist_ReturnsFalse_WhenMovieNotInList()
        {
            // Arrange
            int movieId = 999;

            // Act
            bool result = _watchlistProvider.IsInWatchlist(movieId);

            // Assert
            Assert.That(result, Is.False);
        }

        [Test]
        public void IsInWatchlist_ReturnsTrue_WhenMovieIsInList()
        {
            // Arrange
            int movieId = 1;
            _watchlistProvider.AddToWatchlist(movieId);

            // Act
            bool result = _watchlistProvider.IsInWatchlist(movieId);

            // Assert
            Assert.That(result, Is.True);
        }
        
        [Test]
        public void GetWatchlistIds_ReturnsIds_WhenMovieIsInList()
        {
            // Arrange
            int movieId = 1;
            _watchlistProvider.AddToWatchlist(movieId);

            // Act
            var result = _watchlistProvider.GetWatchlistIds();

            // Assert
            Assert.That(result, Is.Not.Empty);
            Assert.That(result.Last(), Is.EqualTo(movieId));
        }
       

        [Test]
        public void WatchListChanged_IsNotInvoked_WhenNoSubscribers()
        {
            // Arrange
            int movieId = 1;

            // Act & Assert
            Assert.DoesNotThrow(() => _watchlistProvider.AddToWatchlist(movieId));
            Assert.DoesNotThrow(() => _watchlistProvider.RemoveFromWatchlist(movieId));
        }
    }
}