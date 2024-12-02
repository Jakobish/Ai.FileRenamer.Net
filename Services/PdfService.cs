using System.Net.Http.Json;
using System.Text;
using System.Text.RegularExpressions;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;

namespace FileRenamerProject.Services;

public class PdfService : IPdfService
{
    private readonly ILogger<PdfService> _logger;
    private readonly IConfiguration _configuration;
    private readonly HttpClient _httpClient;
    private readonly INameSuggestionCache _cache;
    private readonly IFileLogger _fileLogger;

    public PdfService(
        HttpClient httpClient,
        IConfiguration configuration,
        ILogger<PdfService> logger,
        INameSuggestionCache cache,
        IFileLogger fileLogger)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        _fileLogger = fileLogger ?? throw new ArgumentNullException(nameof(fileLogger));
    }

    public async Task<string> ExtractTextFromPdfAsync(byte[] pdfBytes)
    {
        try
        {
            return await Task.Run(() =>
            {
                using var stream = new MemoryStream(pdfBytes);
                using var pdfReader = new PdfReader(stream);
                using var pdfDocument = new PdfDocument(pdfReader);

                var textBuilder = new StringBuilder();

                for (int i = 1; i <= pdfDocument.GetNumberOfPages(); i++)
                {
                    var page = pdfDocument.GetPage(i);
                    var text = PdfTextExtractor.GetTextFromPage(page);
                    textBuilder.Append(text);
                }

                return textBuilder.ToString();
            });
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "ExtractTextFromPdfAsync failed: {Message}", ex.Message);
            throw;
        }
    }

    public async Task<string> GetSuggestedNameFromAIAsync(string fileName, string content)
    {
        try
        {
            // Check cache first
            var cachedSuggestion = _cache.GetCachedSuggestion(content);
            if (!string.IsNullOrEmpty(cachedSuggestion))
            {
                _logger.LogInformation("Retrieved name suggestion from cache");
                return cachedSuggestion;
            }

            var apiKey = _configuration["Gemini:ApiKey"] ??
                throw new InvalidOperationException("Gemini API key not found");

            await _fileLogger.LogAsync($"Using Gemini API key: {apiKey}", LogLevel.Debug);
            var suggestion = await GetGeminiSuggestionAsync(content, apiKey);
            suggestion = SanitizeFileName(suggestion);
            _cache.CacheSuggestion(content, suggestion);
            return suggestion;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetSuggestedNameFromAIAsync failed for file {FileName}: {Message}", fileName, ex.Message);
            throw;
        }
    }

    private async Task<string> GetGeminiSuggestionAsync(string content, string apiKey)
    {
        try
        {
            var payload = new
            {
                contents = new[]
                {
                    new
                    {
                        parts = new[]
                        {
                            new
                            {
                                text = $"Generate a concise filename for a PDF with this content: {content}. The filename should be descriptive, professional, and follow these rules: 1. Use underscores instead of spaces 2. No special characters except underscores and hyphens 3. Maximum 50 characters 4. All lowercase 5. Must end in .pdf"
                            }
                        }
                    }
                }
            };

            var response = await _httpClient.PostAsJsonAsync(
                $"https://generativelanguage.googleapis.com/v1/models/gemini-pro:generateContent?key={apiKey}",
                payload
            );

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                await _fileLogger.LogAsync($"Gemini API call failed: {error}", LogLevel.Error);
                throw new InvalidOperationException($"Gemini API call failed: {error}");
            }

            var result = await response.Content.ReadFromJsonAsync<dynamic>();

            // Add explicit null checks with detailed logging
            if (result == null)
            {
                _logger.LogError("Gemini API response was null");
                throw new InvalidOperationException("No response received from Gemini API");
            }

            if (result.candidates == null || result.candidates.Count == 0)
            {
                _logger.LogError("No candidates found in Gemini API response");
                throw new InvalidOperationException("No candidates in Gemini API response");
            }

            var suggestedName = result?.candidates?.FirstOrDefault()?.content?.parts?.FirstOrDefault()?.text;

            if (string.IsNullOrWhiteSpace(suggestedName))
            {
                _logger.LogError("Suggested name is null or empty after Gemini API call");
                throw new InvalidOperationException("Gemini returned empty or invalid response");
            }

            return suggestedName;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Gemini API call failed: {ex.Message}");
            throw;
        }
    }

    private string SanitizeFileName(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return string.Empty;

        // Remove any file extension if present
        input = System.IO.Path.GetFileNameWithoutExtension(input);

        // Convert to lowercase
        input = input.ToLowerInvariant();
        
        // Remove special characters (except underscores and hyphens)
        input = Regex.Replace(input, @"[^a-z0-9_\-]", "");
        
        // Ensure max length (excluding .pdf extension)
        const int maxLength = 50 - 4; // 4 is length of ".pdf"
        if (input.Length > maxLength)
        {
            input = input[..maxLength];
        }

        // Ensure it ends with .pdf
        return input + ".pdf";
    }
}
