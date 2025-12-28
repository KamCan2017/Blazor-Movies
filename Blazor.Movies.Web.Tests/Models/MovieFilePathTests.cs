using Blazor.Movies.Web.Models;

namespace Blazor.Movies.Web.Tests.Models;

[TestFixture]
public class MovieFilePathTests
{
    private const string ExpectedFileName = "movies.json";
    
    [Test]
    public void GetFilePath_ReturnsValidPath()
    {
        // Act
        var filePath = MovieFilePath.GetFilePath();

        // Assert
        // 1. Path should not be null or empty
        Assert.That(filePath, Is.Not.Null.And.Not.Empty, "The file path should not be null or empty.");

        // 2. Path should be absolute (starts with a drive letter/root)
        Assert.That(Path.IsPathRooted(filePath), Is.True, "The file path should be rooted/absolute.");

        // 3. Path should end with the correct file name
        Assert.That(filePath.EndsWith(ExpectedFileName, StringComparison.OrdinalIgnoreCase), Is.True, 
            $"The file path must end with '{ExpectedFileName}'.");
    }

    [Test]
    public void GetFilePath_UsesCurrentDirectory()
    {
        // Arrange
        var currentDir = Environment.CurrentDirectory;

        // Act
        var filePath = MovieFilePath.GetFilePath();

        // Assert
        // The file path should start with the current directory path (using Path.Combine logic)
        // We use Path.Combine again to ensure we are comparing against the correctly combined path, 
        // accounting for directory separators.
        var expectedPath = Path.Combine(currentDir, ExpectedFileName);
        
        Assert.That(filePath, Is.EqualTo(expectedPath), 
            "The file path should correctly combine Environment.CurrentDirectory with the file name.");
    }
}