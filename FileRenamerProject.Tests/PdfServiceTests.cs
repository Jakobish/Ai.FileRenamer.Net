using System;
using System.Net.Http;
using FileRenamerProject.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace FileRenamerProject.Tests;

public class PdfServiceTests
{
    private readonly Mock<ILogger<PdfService>> _loggerMock;
    private readonly Mock<IConfiguration> _configMock;
    private readonly HttpClient _httpClient;
    private readonly Mock<INameSuggestionCache> _cacheMock;
    private readonly PdfService _pdfService;

    public PdfServiceTests()
    {
        _loggerMock = new Mock<ILogger<PdfService>>();
        _configMock = new Mock<IConfiguration>();
        _httpClient = new HttpClient();
        _cacheMock = new Mock<INameSuggestionCache>();

        // Setup configuration mock
        _configMock.Setup(x => x["OpenAI:ApiKey"]).Returns("test-api-key");
        _configMock.Setup(x => x["Gemini:ApiKey"]).Returns("test-api-key");

        _pdfService = new PdfService(_httpClient, _configMock.Object, _loggerMock.Object, _cacheMock.Object);
    }

    [Theory]
    [InlineData("test file.txt", "test_file.pdf")]
    [InlineData("TEST FILE.PDF", "test_file.pdf")]
    [InlineData("test@#$%file.pdf", "testfile.pdf")]
    [InlineData("very_long_file_name_that_exceeds_the_maximum_length_limit.pdf", "very_long_file_name_that_exceeds_the_maximum_length_l.pdf")]
    [InlineData("file with spaces.pdf", "file_with_spaces.pdf")]
    [InlineData("file-with-hyphens.pdf", "file-with-hyphens.pdf")]
    [InlineData("file_with_underscores.pdf", "file_with_underscores.pdf")]
    [InlineData("mixed@#$CASE__file.pdf", "mixedcase__file.pdf")]
    public void SanitizeFileName_ShouldReturnValidFileName(string input, string expected)
    {
        // Act
        var result = CallSanitizeFileName(input);

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void SanitizeFileName_WithNullOrEmpty_ShouldNotThrowException(string input)
    {
        // Act & Assert
        var exception = Record.Exception(() => CallSanitizeFileName(input));
        Assert.Null(exception);
    }

    [Fact]
    public void SanitizeFileName_WithoutExtension_ShouldAddPdfExtension()
    {
        // Arrange
        var input = "test_file";

        // Act
        var result = CallSanitizeFileName(input);

        // Assert
        Assert.EndsWith(".pdf", result);
    }

    [Fact]
    public void SanitizeFileName_WithDifferentExtension_ShouldChangeToPdf()
    {
        // Arrange
        var input = "test_file.txt";

        // Act
        var result = CallSanitizeFileName(input);

        // Assert
        Assert.EndsWith(".pdf", result);
    }

    // Helper method to call the private SanitizeFileName method using reflection
    private string CallSanitizeFileName(string input)
    {
        var methodInfo = typeof(PdfService).GetMethod("SanitizeFileName", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        
        return (string)methodInfo.Invoke(_pdfService, new object[] { input });
    }
}
