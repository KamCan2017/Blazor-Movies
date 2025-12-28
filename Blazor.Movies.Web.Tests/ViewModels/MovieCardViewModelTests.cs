using Blazor.Movies.Web.Models;
using Blazor.Movies.Web.Services;
using Blazor.Movies.Web.StateObservers;
using Blazor.Movies.Web.ViewModels;
using Moq;

namespace Blazor.Movies.Web.Tests.ViewModels
{
    [TestFixture]
    public class MovieCardViewModelTests
    {
        private Mock<IWatchlistProvider> _mockWatchlistProvider;
        private Mock<IStateObserver> _mockStateObserver;
        private MovieCardViewModel _viewModel;

        [SetUp]
        public void SetUp()
        {
            _mockWatchlistProvider = new Mock<IWatchlistProvider>();
            _mockStateObserver = new Mock<IStateObserver>();
            _viewModel = new MovieCardViewModel(_mockWatchlistProvider.Object, _mockStateObserver.Object);
        }

        #region Constructor Tests

        [Test]
        public void Constructor_ThrowsArgumentNullException_WhenWatchlistProviderIsNull()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => new MovieCardViewModel(null!, _mockStateObserver.Object));
        }
        
        [Test]
        public void Constructor_ThrowsArgumentNullException_WhenStateObserverIsNull()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => new MovieCardViewModel(_mockWatchlistProvider.Object,null!));
        }

        [Test]
        public void Constructor_InitializesShowImageToTrue()
        {
            // Assert
            Assert.That(_viewModel.ShowImage, Is.True);
        }

        [Test]
        public void Constructor_InitializesWithEmptyMovie()
        {
            // Assert
            Assert.That(_viewModel.Movie, Is.Not.Null);
        }

        #endregion
       

        #region HandleErrorByLoadingImage Tests

        [Test]
        public void HandleErrorByLoadingImage_SetsShowImageToFalse()
        {
            // Arrange
            var movie = new Movie { Id = 1, Title = "Test Movie", Image = "test.jpg" };
            _viewModel.Movie = movie;

            // Act
            _viewModel.HandleErrorByLoadingImage();

            // Assert
            Assert.That(_viewModel.ShowImage, Is.False);
        }

      
        #endregion

        #region ToggleWatchlist Tests

        [Test]
        public void ToggleWatchlist_AddsToWatchlist_WhenIsInWatchlistIsTrue()
        {
            // Arrange
            var movie = new Movie { Id = 1, Title = "Test Movie" };
            _mockWatchlistProvider.Setup(x => x.IsInWatchlist(1)).Returns(false);
            _viewModel.Movie = movie;
            _viewModel.Movie.IsInWatchList = true;

            // Act
            _viewModel.ToggleWatchlist();

            // Assert
            _mockWatchlistProvider.Verify(x => x.AddToWatchlist(1), Times.Once);
        }

        [Test]
        public void ToggleWatchlist_RemovesFromWatchlist_WhenIsInWatchlistIsFalse()
        {
            // Arrange
            var movie = new Movie { Id = 1, Title = "Test Movie" };
            _mockWatchlistProvider.Setup(x => x.IsInWatchlist(1)).Returns(true);
            _viewModel.Movie = movie;
            _viewModel.Movie.IsInWatchList = false;

            // Act
            _viewModel.ToggleWatchlist();

            // Assert
            _mockWatchlistProvider.Verify(x => x.RemoveFromWatchlist(1), Times.Once);
        }

        [Test]
        public void ToggleWatchlist_DoesNotRemove_WhenIsInWatchlistIsTrue()
        {
            // Arrange
            var movie = new Movie { Id = 1, Title = "Test Movie" };
            _viewModel.Movie = movie;
            _viewModel.Movie.IsInWatchList = true;

            // Act
            _viewModel.ToggleWatchlist();

            // Assert
            _mockWatchlistProvider.Verify(x => x.RemoveFromWatchlist(It.IsAny<int>()), Times.Never);
        }

        [Test]
        public void ToggleWatchlist_DoesNotAdd_WhenIsInWatchlistIsFalse()
        {
            // Arrange
            var movie = new Movie { Id = 1, Title = "Test Movie" };
            _viewModel.Movie = movie;
            _viewModel.Movie.IsInWatchList = false;

            // Act
            _viewModel.ToggleWatchlist();

            // Assert
            _mockWatchlistProvider.Verify(x => x.AddToWatchlist(It.IsAny<int>()), Times.Never);
        }

      
        #endregion

        #region GetRatingClass Tests

        [Test]
        [TestCase(10.0, "rating-good")]
        [TestCase(9.5, "rating-good")]
        [TestCase(8.0, "rating-good")]
        [TestCase(8.1, "rating-good")]
        public void GetRatingClass_ReturnsGood_WhenRatingIsEightOrAbove(double rating, string expected)
        {
            // Act
            var result = MovieCardViewModel.GetRatingClass(rating);

            // Assert
            Assert.That(result, Is.EqualTo(expected));
        }

        [Test]
        [TestCase(7.9, "rating-ok")]
        [TestCase(7.0, "rating-ok")]
        [TestCase(6.5, "rating-ok")]
        [TestCase(5.0, "rating-ok")]
        [TestCase(5.1, "rating-ok")]
        public void GetRatingClass_ReturnsOk_WhenRatingIsBetweenFiveAndEight(double rating, string expected)
        {
            // Act
            var result = MovieCardViewModel.GetRatingClass(rating);

            // Assert
            Assert.That(result, Is.EqualTo(expected));
        }

        [Test]
        [TestCase(4.9, "rating-bad")]
        [TestCase(4.0, "rating-bad")]
        [TestCase(2.5, "rating-bad")]
        [TestCase(1.0, "rating-bad")]
        [TestCase(0.0, "rating-bad")]
        public void GetRatingClass_ReturnsBad_WhenRatingIsBelowFive(double rating, string expected)
        {
            // Act
            var result = MovieCardViewModel.GetRatingClass(rating);

            // Assert
            Assert.That(result, Is.EqualTo(expected));
        }

        [Test]
        public void GetRatingClass_ReturnsBad_ForNegativeRating()
        {
            // Act
            var result = MovieCardViewModel.GetRatingClass(-1.0);

            // Assert
            Assert.That(result, Is.EqualTo("rating-bad"));
        }

        #endregion

        #region Integration Tests

        [Test]
        public void ViewModel_WorksEndToEnd_AddingMovieToWatchlist()
        {
            // Arrange
            var movie = new Movie { Id = 1, Title = "Test Movie", Rating = 8.5 };
            _mockWatchlistProvider.Setup(x => x.IsInWatchlist(1)).Returns(false);

            // Act - Set movie
            _viewModel.Movie = movie;
            
            // Act - Toggle to add
            _viewModel.Movie.IsInWatchList = true;
            _viewModel.ToggleWatchlist();

            // Assert
            Assert.That(_viewModel.Movie.Title, Is.EqualTo("Test Movie"));
            _mockWatchlistProvider.Verify(x => x.AddToWatchlist(1), Times.Once);
        }

        [Test]
        public void ViewModel_WorksEndToEnd_RemovingMovieFromWatchlist()
        {
            // Arrange
            var movie = new Movie { Id = 1, Title = "Test Movie", Rating = 8.5 };
            _mockWatchlistProvider.Setup(x => x.IsInWatchlist(1)).Returns(true);

            // Act - Set movie
            _viewModel.Movie = movie;
            
            // Act - Toggle to remove
            _viewModel.Movie.IsInWatchList = false;
            _viewModel.ToggleWatchlist();

            // Assert
            Assert.That(_viewModel.Movie.IsInWatchList, Is.False);
            _mockWatchlistProvider.Verify(x => x.RemoveFromWatchlist(1), Times.Once);
        }

        [Test]
        public void ViewModel_HandlesImageError_AndMaintainsOtherState()
        {
            // Arrange
            var movie = new Movie { Id = 1, Title = "Test Movie", Image = "test.jpg" };
            _mockWatchlistProvider.Setup(x => x.IsInWatchlist(1)).Returns(true);
            _viewModel.Movie = movie;

            // Act
            _viewModel.HandleErrorByLoadingImage();

            // Assert
            Assert.That(_viewModel.ShowImage, Is.False);
            Assert.That(_viewModel.Movie.Title, Is.EqualTo("Test Movie"));
        }

        #endregion
    }
}