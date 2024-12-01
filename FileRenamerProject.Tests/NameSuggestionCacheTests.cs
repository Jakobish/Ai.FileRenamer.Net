using FileRenamerProject.Services;
using Xunit;
using Xunit.Sdk;

namespace FileRenamerProject.Tests;

public class NameSuggestionCacheTests
{
    private readonly INameSuggestionCache _cache;

    public NameSuggestionCacheTests()
    {
        _cache = new NameSuggestionCache();
    }

    [Fact]
    public void GetCachedSuggestion_WhenEmpty_ReturnsNull()
    {
        // Act
        var result = _cache.GetCachedSuggestion("test content");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void CacheSuggestion_WhenCalled_StoresSuggestion()
    {
        // Arrange
        var content = "test content";
        var suggestion = "test_file.pdf";

        // Act
        _cache.CacheSuggestion(content, suggestion);
        var result = _cache.GetCachedSuggestion(content);

        // Assert
        Assert.Equal(suggestion, result);
    }

    [Fact]
    public void Clear_WhenCalled_RemovesAllEntries()
    {
        // Arrange
        _cache.CacheSuggestion("content1", "suggestion1");
        _cache.CacheSuggestion("content2", "suggestion2");

        // Act
        _cache.Clear();
        var result1 = _cache.GetCachedSuggestion("content1");
        var result2 = _cache.GetCachedSuggestion("content2");

        // Assert
        Assert.Null(result1);
        Assert.Null(result2);
    }

    [Theory]
    [InlineData(null, "suggestion")]
    [InlineData("", "suggestion")]
    [InlineData("content", null)]
    [InlineData("content", "")]
    public void CacheSuggestion_WithNullOrEmpty_DoesNotThrow(string content, string suggestion)
    {
        // Act & Assert
        var exception = Record.Exception(() => _cache.CacheSuggestion(content, suggestion));
        Assert.Null(exception);
    }

    [Fact]
    public void CacheSuggestion_WithSameContent_ReturnsSameSuggestion()
    {
        // Arrange
        var content = "test content";
        var suggestion1 = "suggestion1.pdf";
        var suggestion2 = "suggestion2.pdf";

        // Act
        _cache.CacheSuggestion(content, suggestion1);
        var result1 = _cache.GetCachedSuggestion(content);
        _cache.CacheSuggestion(content, suggestion2);
        var result2 = _cache.GetCachedSuggestion(content);

        // Assert
        Assert.Equal(suggestion1, result1);
        Assert.Equal(suggestion1, result2); // Should still return the first suggestion
    }
}
