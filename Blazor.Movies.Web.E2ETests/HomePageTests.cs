using Microsoft.Playwright;

namespace Blazor.Movies.Web.E2ETests
{
    [TestFixture]
    [Category("E2E")]
    public class HomePagePlaywrightTests : PageTest
    {
        private const string BaseUrl = "http://localhost:5082"; // Update with your app URL
        
        [SetUp]
        public async Task SetUp()
        {
            // Navigate to the home page before each test
            await Page.GotoAsync(BaseUrl);
        }

        #region Page Load Tests

        [Test]
        public async Task HomePage_LoadsSuccessfully()
        {
            // Assert
            await Expect(Page).ToHaveTitleAsync(new Regex(".*Movies.*|.*Home.*"));
            await Expect(Page).ToHaveURLAsync(new Regex($"{BaseUrl}.*"));
        }

        [Test]
        public async Task HomePage_DisplaysTitle()
        {
            // Assert
            var heading = Page.Locator("h1").First;
            await Expect(heading).ToBeVisibleAsync();
            await Expect(heading).ToContainTextAsync(new Regex("Movies|Home"));
        }

        #endregion

        #region Movie Display Tests

        [Test]
        public async Task HomePage_DisplaysMovieCards()
        {
            // Wait for movies to load
            await Page.WaitForSelectorAsync(".movie-card", new PageWaitForSelectorOptions 
            { 
                Timeout = 5000 
            });

            // Assert
            var movieCards = Page.Locator(".movie-card");
            await Expect(movieCards).Not.ToHaveCountAsync(0);
        }

        [Test]
        public async Task HomePage_MovieCards_ContainRequiredElements()
        {
            // Wait for at least one movie card
            await Page.WaitForSelectorAsync(".movie-card", new PageWaitForSelectorOptions 
            { 
                Timeout = 5000 
            });

            var firstCard = Page.Locator(".movie-card").First;

            // Assert - Movie card should contain title, rating, and genre
            await Expect(firstCard).ToBeVisibleAsync();
            
            // Check for movie title (adjust selector based on your HTML structure)
            var title = firstCard.Locator(".movie-title, h2, h3");
            await Expect(title.First).ToBeVisibleAsync();

            // Check for rating (adjust selector based on your HTML structure)
            var rating = firstCard.Locator(".movie-rating, .rating");
            await Expect(rating.First).ToBeVisibleAsync();

            // Check for genre (adjust selector based on your HTML structure)
            var genre = firstCard.Locator(".movie-genre, .genre");
            await Expect(genre.First).ToBeVisibleAsync();
        }

        [Test]
        public async Task HomePage_DisplaysMovieImages()
        {
            // Wait for movies to load
            await Page.WaitForSelectorAsync(".movie-card", new PageWaitForSelectorOptions 
            { 
                Timeout = 5000 
            });

            var firstCard = Page.Locator(".movie-card").First;
            var image = firstCard.Locator("img");

            // Assert
            await Expect(image).ToBeVisibleAsync();
            await Expect(image).ToHaveAttributeAsync("src", new Regex(".+"));
        }

        #endregion

        #region Search Functionality Tests

        [Test]
        public async Task HomePage_HasSearchInput()
        {
            // Assert
            var searchInput = Page.Locator("input[type='text']").First;
            await Expect(searchInput).ToBeVisibleAsync();
        }

        [Test]
        public async Task HomePage_SearchFiltersMovies()
        {
            // Wait for movies to load
            await Page.WaitForSelectorAsync(".movie-card", new PageWaitForSelectorOptions 
            { 
                Timeout = 5000 
            });

            // Get initial count of movies
            var initialMovieCards = Page.Locator(".movie-card");
            var initialCount = await initialMovieCards.CountAsync();

            // Find search input (adjust selector based on your implementation)
            var searchInput = Page.Locator("input[type='text']").First;
            
            // Act - Enter search term
            await searchInput.FillAsync("Matrix");
            await searchInput.PressAsync("Enter");
            
            // Wait a bit for filtering to occur
            await Page.WaitForTimeoutAsync(500);

            // Assert - Movie count should change (or at least be filtered)
            var filteredMovieCards = Page.Locator(".movie-card");
            var filteredCount = await filteredMovieCards.CountAsync();

            // Verify that results contain the search term
            if (filteredCount > 0)
            {
                var firstTitle = filteredMovieCards.First.Locator(".movie-title, h2, h3").First;
                await Expect(firstTitle).ToContainTextAsync(new Regex("Matrix", RegexOptions.IgnoreCase));
            }
        }

        [Test]
        public async Task HomePage_SearchIsCaseInsensitive()
        {
            // Wait for movies to load
            await Page.WaitForSelectorAsync(".movie-card", new PageWaitForSelectorOptions 
            { 
                Timeout = 5000 
            });

            var searchInput = Page.Locator("input[type='text']").First;
            
            // Act - Search with lowercase
            await searchInput.FillAsync("matrix");
            await searchInput.PressAsync("Enter");
            await Page.WaitForTimeoutAsync(500);

            // Assert
            var movieCards = Page.Locator(".movie-card");
            var count = await movieCards.CountAsync();

            if (count > 0)
            {
                var firstTitle = movieCards.First.Locator(".movie-title, h2, h3").First;
                await Expect(firstTitle).ToContainTextAsync(new Regex("matrix", RegexOptions.IgnoreCase));
            }
        }

        [Test]
        public async Task HomePage_ClearSearchShowsAllMovies()
        {
            // Wait for movies to load
            await Page.WaitForSelectorAsync(".movie-card", new PageWaitForSelectorOptions 
            { 
                Timeout = 5000 
            });

            var initialMovieCards = Page.Locator(".movie-card");
            var initialCount = await initialMovieCards.CountAsync();

            var searchInput = Page.Locator("input[type='text']").First;
            
            // Act - Search and then clear
            await searchInput.FillAsync("Matrix");
            await searchInput.PressAsync("Enter");
            await Page.WaitForTimeoutAsync(500);

            await searchInput.ClearAsync();
            await searchInput.PressAsync("Enter");
            await Page.WaitForTimeoutAsync(500);

            // Assert - Should show all movies again
            var finalMovieCards = Page.Locator(".movie-card");
            var finalCount = await finalMovieCards.CountAsync();

            Assert.That(finalCount, Is.GreaterThanOrEqualTo(initialCount));
        }

        #endregion

        #region Genre Filter Tests

        [Test]
        public async Task HomePage_HasGenreFilter()
        {
            // Assert
            var genreSelect = Page.Locator("select").First;
            await Expect(genreSelect).ToBeVisibleAsync();
        }

        [Test]
        public async Task HomePage_GenreFilterWorks()
        {
            // Wait for movies to load
            await Page.WaitForSelectorAsync(".movie-card", new PageWaitForSelectorOptions 
            { 
                Timeout = 5000 
            });

            // Find genre select (adjust selector based on your implementation)
            var genreSelect = Page.Locator("select").First;

            // Act - Select a genre (e.g., "Sci-Fi")
            await genreSelect.SelectOptionAsync("Sci-Fi");
            await Page.WaitForTimeoutAsync(500);

            // Assert - All visible movie cards should be Sci-Fi genre
            var movieCards = Page.Locator(".movie-card");
            var count = await movieCards.CountAsync();

            if (count > 0)
            {
                for (int i = 0; i < Math.Min(count, 3); i++) // Check first 3 movies
                {
                    var genre = movieCards.Nth(i).Locator(".movie-genre, .genre").First;
                    await Expect(genre).ToContainTextAsync("Sci-Fi");
                }
            }
        }

        [Test]
        public async Task HomePage_ResetGenreFilterShowsAllMovies()
        {
            // Wait for movies to load
            await Page.WaitForSelectorAsync(".movie-card", new PageWaitForSelectorOptions 
            { 
                Timeout = 5000 
            });

            var initialMovieCards = Page.Locator(".movie-card");
            var initialCount = await initialMovieCards.CountAsync();

            var genreSelect = Page.Locator("select").First;

            // Act - Filter and then reset
            await genreSelect.SelectOptionAsync("Sci-Fi");
            await Page.WaitForTimeoutAsync(500);

            await genreSelect.SelectOptionAsync("All"); // Or "" depending on your implementation
            await Page.WaitForTimeoutAsync(500);

            // Assert
            var finalMovieCards = Page.Locator(".movie-card");
            var finalCount = await finalMovieCards.CountAsync();

            Assert.That(finalCount, Is.EqualTo(initialCount));
        }

        #endregion

        #region Rating Filter Tests

        [Test]
        public async Task HomePage_FiltersMoviesByRating()
        {
            // Wait for movies to load
            await Page.WaitForSelectorAsync(".movie-card", new PageWaitForSelectorOptions 
            { 
                Timeout = 5000 
            });

            // Find rating filter (adjust selector based on your implementation)
            var ratingSelect = Page.Locator("select[name='rating'], select.rating-filter").First;

            // Act - Select "Good" rating filter (Rating >= 8)
            await ratingSelect.SelectOptionAsync("Good");
            await Page.WaitForTimeoutAsync(500);

            // Assert - Check that visible movies have rating >= 8
            var movieCards = Page.Locator(".movie-card");
            var count = await movieCards.CountAsync();

            if (count > 0)
            {
                var firstRating = movieCards.First.Locator(".movie-rating, .rating").First;
                var ratingText = await firstRating.TextContentAsync();
                var ratingValue = double.Parse(ratingText?.Trim() ?? "0");
                Assert.That(ratingValue, Is.GreaterThanOrEqualTo(8.0));
            }
        }

        #endregion

        #region Watchlist Integration Tests

        [Test]
        public async Task HomePage_MovieCards_HaveWatchlistToggle()
        {
            // Wait for movies to load
            await Page.WaitForSelectorAsync(".movie-card", new PageWaitForSelectorOptions 
            { 
                Timeout = 5000 
            });

            var firstCard = Page.Locator(".movie-card").First;
            
            // Assert - Should have a checkbox or button for watchlist
            var watchlistToggle = firstCard.Locator("input[type='checkbox'], button").First;
            await Expect(watchlistToggle).ToBeVisibleAsync();
        }

        [Test]
        public async Task HomePage_CanToggleWatchlist()
        {
            // Wait for movies to load
            await Page.WaitForSelectorAsync(".movie-card", new PageWaitForSelectorOptions 
            { 
                Timeout = 5000 
            });

            var firstCard = Page.Locator(".movie-card").First;
            var watchlistCheckbox = firstCard.Locator("input[type='checkbox']").First;

            // Get initial state
            var isCheckedBefore = await watchlistCheckbox.IsCheckedAsync();

            // Act - Toggle watchlist
            await watchlistCheckbox.ClickAsync();
            await Page.WaitForTimeoutAsync(300);

            // Assert - State should be opposite
            var isCheckedAfter = await watchlistCheckbox.IsCheckedAsync();
            Assert.That(isCheckedAfter, Is.Not.EqualTo(isCheckedBefore));
        }

        #endregion

        #region Combined Filter Tests

        [Test]
        public async Task HomePage_AppliesMultipleFiltersSimultaneously()
        {
            // Wait for movies to load
            await Page.WaitForSelectorAsync(".movie-card", new PageWaitForSelectorOptions 
            { 
                Timeout = 5000 
            });

            // Apply multiple filters
            var searchInput = Page.Locator("input[type='text']").First;
            await searchInput.FillAsync("Action");
            
            var genreSelect = Page.Locator("select").First;
            await genreSelect.SelectOptionAsync("Action");

            await Page.WaitForTimeoutAsync(500);

            // Assert - Results should match all filters
            var movieCards = Page.Locator(".movie-card");
            var count = await movieCards.CountAsync();

            if (count > 0)
            {
                var firstTitle = movieCards.First.Locator(".movie-title, h2, h3").First;
                await Expect(firstTitle).ToContainTextAsync(new Regex("Action", RegexOptions.IgnoreCase));

                var firstGenre = movieCards.First.Locator(".movie-genre, .genre").First;
                await Expect(firstGenre).ToContainTextAsync("Action");
            }
        }

        #endregion

        #region Responsive and Navigation Tests

        [Test]
        public async Task HomePage_IsResponsive_Mobile()
        {
            // Set mobile viewport
            await Page.SetViewportSizeAsync(375, 667);

            // Wait for movies to load
            await Page.WaitForSelectorAsync(".movie-card", new PageWaitForSelectorOptions 
            { 
                Timeout = 5000,
                State = WaitForSelectorState.Visible
            });

            // Assert - Page should still be functional on mobile
            var movieCards = Page.Locator(".movie-card");
            await Expect(movieCards.First).ToBeVisibleAsync();
        }

        [Test]
        public async Task HomePage_IsResponsive_Tablet()
        {
            // Set tablet viewport
            await Page.SetViewportSizeAsync(768, 1024);

            // Wait for movies to load
            await Page.WaitForSelectorAsync(".movie-card", new PageWaitForSelectorOptions 
            { 
                Timeout = 5000,
                State = WaitForSelectorState.Visible
            });

            // Assert
            var movieCards = Page.Locator(".movie-card");
            await Expect(movieCards.First).ToBeVisibleAsync();
        }

        #endregion

        #region Performance Tests

        [Test]
        public async Task HomePage_LoadsWithinAcceptableTime()
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            
            // Navigate and wait for movies to load
            await Page.GotoAsync(BaseUrl);
            await Page.WaitForSelectorAsync(".movie-card", new PageWaitForSelectorOptions 
            { 
                Timeout = 5000 
            });
            
            stopwatch.Stop();

            // Assert - Page should load within 5 seconds
            Assert.That(stopwatch.ElapsedMilliseconds, Is.LessThan(5000));
        }

        #endregion

        #region Accessibility Tests

        [Test]
        public async Task HomePage_HasAccessibleElements()
        {
            // Wait for page to load
            await Page.WaitForSelectorAsync(".movie-card", new PageWaitForSelectorOptions 
            { 
                Timeout = 5000 
            });

            // Assert - Check for important accessibility features
            var searchInput = Page.Locator("input[type='text']").First;
            await Expect(searchInput).ToHaveAttributeAsync("placeholder", new Regex(".+"));

            // Images should have alt text
            var images = Page.Locator("img");
            var firstImage = images.First;
            await Expect(firstImage).ToHaveAttributeAsync("alt", new Regex(".*"));
        }

        #endregion
    }
}