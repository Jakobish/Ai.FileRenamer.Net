using System.Net.Http.Json;
using System.Text;
using System.Text.RegularExpressions;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using OpenAI_API;
using Microsoft.Extensions.Logging;
using FileRenamerProject.Data;
using FileRenamerProject.Services;

namespace FileRenamerProject.Services;

public class PdfService : IPdfService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<PdfService> _logger;
    private readonly OpenAIAPI _openAIApi;
    private readonly INameSuggestionCache _cache;

    public PdfService(
        HttpClient httpClient, 
        IConfiguration configuration, 
        ILogger<PdfService> logger,
        INameSuggestionCache cache)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;
        _cache = cache;
        
        var apiKey = configuration["OpenAI:ApiKey"] ?? throw new InvalidOperationException("OpenAI API key not found");
        _openAIApi = new OpenAIAPI(apiKey);
    }

    public async Task<string> ExtractTextFromPdfAsync(byte[] pdfBytes)
    {
        try
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
        }
        catch (Exception)
        {
            // Log the exception or handle it appropriately
            return string.Empty;
        }
    }

    public async Task<string> GetSuggestedNameFromAIAsync(string content)
    {
        try
        {
            var apiKey = _configuration["GeminiApiKey"];
            if (string.IsNullOrEmpty(apiKey))
            {
                throw new InvalidOperationException("Gemini API key not configured");
            }

            // Call the hypothetical Gemini API
            var suggestedName = await GetGeminiSuggestionAsync(content, apiKey); 

            // Clean up the suggested name using more efficient regex
            suggestedName = Regex.Replace(suggestedName, @"[^\w\d]", "_");
            suggestedName = Regex.Replace(suggestedName, @"_+", "_");
            suggestedName = suggestedName.Trim('_');

            return suggestedName; 
        }
        catch (GeminiApiException ex)
        {
            // Log or handle Gemini specific exceptions
            throw new Exception($"Gemini API error: {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            throw new Exception($"Error getting suggestion: {ex.Message}", ex);
        }
    }

    public async Task<string> GetSuggestedNameFromAIAsync(string fileName, string content)
    {
        try
        {
            // Check cache first
            var cachedSuggestion = _cache.GetCachedSuggestion(content);
            if (cachedSuggestion != null)
            {
                _logger.LogInformation("Retrieved name suggestion from cache");
                return cachedSuggestion;
            }

            var aiProvider = _configuration["AIProvider"] ?? "OpenAI";
            var apiKey = _configuration[$"{aiProvider}:ApiKey"] ?? throw new InvalidOperationException($"{aiProvider} API key not found");

            string suggestion;
            Exception lastException = null;

            // Try the configured provider first
            try
            {
                suggestion = aiProvider.Equals("OpenAI", StringComparison.OrdinalIgnoreCase)
                    ? await GetOpenAISuggestionAsync(content, apiKey)
                    : await GetGeminiSuggestionAsync(content, apiKey);
                
                // If successful, cache and return
                _cache.CacheSuggestion(content, suggestion);
                return suggestion;
            }
            catch (Exception ex) when (ex.Message.Contains("rate limit", StringComparison.OrdinalIgnoreCase))
            {
                lastException = ex;
                _logger.LogWarning($"{aiProvider} rate limit exceeded, trying fallback provider");
                
                // Try the other provider as fallback
                try
                {
                    var fallbackProvider = aiProvider.Equals("OpenAI", StringComparison.OrdinalIgnoreCase) ? "Gemini" : "OpenAI";
                    var fallbackApiKey = _configuration[$"{fallbackProvider}:ApiKey"];
                    
                    if (string.IsNullOrEmpty(fallbackApiKey))
                    {
                        throw new InvalidOperationException($"No API key found for fallback provider {fallbackProvider}");
                    }

                    suggestion = fallbackProvider.Equals("OpenAI", StringComparison.OrdinalIgnoreCase)
                        ? await GetOpenAISuggestionAsync(content, fallbackApiKey)
                        : await GetGeminiSuggestionAsync(content, fallbackApiKey);
                    
                    // If fallback successful, cache and return
                    _cache.CacheSuggestion(content, suggestion);
                    return suggestion;
                }
                catch (Exception fallbackEx)
                {
                    _logger.LogError($"Fallback provider failed: {fallbackEx.Message}");
                    throw new AggregateException("Both primary and fallback AI providers failed", new[] { lastException, fallbackEx });
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"Failed to get AI suggestion for file {fileName}: {ex.Message}");
            throw new ApplicationException($"Failed to generate name suggestion: {ex.Message}", ex);
        }
    }

    private async Task<string> GetOpenAISuggestionAsync(string content, string apiKey)
    {
        try
        {
            // Check cache first
            var cachedSuggestion = _cache.GetCachedSuggestion(content);
            if (cachedSuggestion != null)
            {
                _logger.LogInformation("Retrieved name suggestion from cache");
                return cachedSuggestion;
            }

            var chat = _openAIApi.Chat.CreateConversation();
            chat.AppendSystemMessage("You are a helpful assistant that suggests concise, descriptive filenames based on PDF content. The filename should be clear, professional, and follow these rules: 1. Use underscores instead of spaces 2. No special characters except underscores and hyphens 3. Maximum 50 characters 4. All lowercase 5. Must end in .pdf");
            chat.AppendUserInput($"Suggest a filename for a PDF with the following content:\n\n{content}");
            
            var suggestedName = await chat.GetResponseFromChatbotAsync();
            
            // Ensure the name follows our rules
            suggestedName = SanitizeFileName(suggestedName);
            
            // Cache the suggestion
            _cache.CacheSuggestion(content, suggestedName);
            
            return suggestedName;
        }
        catch (Exception ex)
        {
            _logger.LogError($"OpenAI API call failed: {ex.Message}");
            throw;
        }
    }

    private async Task<string> GetGeminiSuggestionAsync(string content, string apiKey)
    {
        try
        {
            // Check cache first
            var cachedSuggestion = _cache.GetCachedSuggestion(content);
            if (cachedSuggestion != null)
            {
                _logger.LogInformation("Retrieved name suggestion from cache");
                return cachedSuggestion;
            }

            // Construct the request payload
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

            // Make the API call
            var response = await _httpClient.PostAsJsonAsync(
                $"https://generativelanguage.googleapis.com/v1/models/gemini-pro:generateContent?key={apiKey}",
                payload
            );

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new GeminiApiException($"Gemini API call failed: {error}");
            }

            var result = await response.Content.ReadFromJsonAsync<GeminiResponse>();
            var suggestedName = result?.Candidates?.FirstOrDefault()?.Content?.Parts?.FirstOrDefault()?.Text ?? 
                               throw new GeminiApiException("No valid response from Gemini API");

            // Ensure the name follows our rules
            suggestedName = SanitizeFileName(suggestedName);
            
            // Cache the suggestion
            _cache.CacheSuggestion(content, suggestedName);
            
            return suggestedName;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Gemini API call failed: {ex.Message}");
            throw;
        }
    }

    private string SanitizeFileName(string fileName)
    {
        // Ensure the name ends with .pdf
        if (!fileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
        {
            fileName += ".pdf";
        }

        // Convert to lowercase
        fileName = fileName.ToLower();

        // Replace spaces with underscores
        fileName = fileName.Replace(' ', '_');

        // Remove any special characters except underscores and hyphens
        fileName = Regex.Replace(fileName, "[^a-z0-9_\\-.]", "");

        // Ensure maximum length (including .pdf extension)
        const int maxLength = 50;
        if (fileName.Length > maxLength)
        {
            var extension = System.IO.Path.GetExtension(fileName);
            var nameWithoutExtension = System.IO.Path.GetFileNameWithoutExtension(fileName);
            fileName = nameWithoutExtension.Substring(0, maxLength - extension.Length) + extension;
        }

        return fileName;
    }

    // Gemini API response classes
    private class GeminiResponse
    {
        public List<Candidate>? Candidates { get; set; }
    }

    private class Candidate
    {
        public Content? Content { get; set; }
    }

    private class Content
    {
        public List<Part>? Parts { get; set; }
    }

    private class Part
    {
        public string? Text { get; set; }
    }

    // Gemini specific exception
    public class GeminiApiException : Exception 
    {
        public GeminiApiException(string message) : base(message) { }
    }
}
