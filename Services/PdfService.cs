using System.Net.Http.Json;
using System.Text;
using System.Text.RegularExpressions;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using Microsoft.Extensions.Logging;
using FileRenamerProject.Data;
using FileRenamerProject.Services;
using OpenAI.Chat;
using OpenAI;
using Castle.Components.DictionaryAdapter.Xml;



namespace FileRenamerProject.Services;

public class PdfService : IPdfService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<PdfService> _logger;
    private readonly INameSuggestionCache _cache;

    public PdfService(
        HttpClient httpClient,
        IConfiguration configuration,
        ILogger<PdfService> logger,
        INameSuggestionCache cache)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _configuration = configuration;
        _logger = logger;
        _cache = cache;
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
        catch (Exception ex)
        {
            _logger.LogError($"Failed to extract text from PDF: {ex.Message}");
            throw;
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

            var aiProvider = _configuration["AIProvider"] ?? "Gemini";
            var apiKey = _configuration[$"{aiProvider}:ApiKey"] ??
                throw new InvalidOperationException($"{aiProvider} API key not found");

            string suggestion;
            try
            {
                suggestion = aiProvider.Equals("OpenAI", StringComparison.OrdinalIgnoreCase)
                    ? await GetOpenAISuggestionAsync(content, apiKey)
                    : await GetGeminiSuggestionAsync(content, apiKey);
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"{aiProvider} failed: {ex.Message}. Trying fallback provider...");

                // Try the other provider as fallback
                var fallbackProvider = aiProvider.Equals("OpenAI", StringComparison.OrdinalIgnoreCase) ? "Gemini" : "OpenAI";
                var fallbackApiKey = _configuration[$"{fallbackProvider}:ApiKey"];

                if (string.IsNullOrEmpty(fallbackApiKey))
                {
                    throw new InvalidOperationException($"No API key found for fallback provider {fallbackProvider}");
                }

                suggestion = fallbackProvider.Equals("OpenAI", StringComparison.OrdinalIgnoreCase)
                    ? await GetOpenAISuggestionAsync(content, fallbackApiKey)
                    : await GetGeminiSuggestionAsync(content, fallbackApiKey);
            }

            suggestion = SanitizeFileName(suggestion);
            _cache.CacheSuggestion(content, suggestion);
            return suggestion;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Failed to get AI suggestion for file {fileName}: {ex.Message}");
            throw;
        }
    }

    private async Task<string> GetOpenAISuggestionAsync(string content, string apiKey)
    {
        try
        {
            var api = new OpenAIClient(apiKey);

            var chatRequest = new ChatRequest
            
            
            {
                Model = "gpt-3.5-turbo",
                Temperature = 0.7f,
                Messages = new List<ChatMessage>
                {
                    new ChatMessage(ChatMessageRole.System, "You are a helpful assistant that suggests concise, descriptive filenames based on PDF content. The filename should be clear, professional, and follow these rules: 1. Use underscores instead of spaces 2. No special characters except underscores and hyphens 3. Maximum 50 characters 4. All lowercase 5. Must end in .pdf"),
                    new ChatMessage(ChatMessageRole.User, $"Suggest a filename for a PDF with the following content: {content}")
                }
            };

            var chatCompletion = await api.ChatCompletions.CreateAsync(chatRequest);
            return chatCompletion.Choices[0].Message.Content.Trim();
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
                throw new InvalidOperationException($"Gemini API call failed: {error}");
            }

            var result = await response.Content.ReadFromJsonAsync<dynamic>();
            var suggestedName = result?.candidates?[0]?.content?.parts?[0]?.text?.ToString();

            if (string.IsNullOrWhiteSpace(suggestedName))
            {
                throw new InvalidOperationException("Gemini returned empty response");
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
            throw new ArgumentException("Input string is empty or whitespace", nameof(input));

        // Remove any file extension if present
        input = Path.GetFileNameWithoutExtension(input);

        // Convert to lowercase and replace spaces/special chars with underscore
        input = input.ToLowerInvariant();
        input = Regex.Replace(input, @"[^\w\-]", "_");
        input = Regex.Replace(input, @"_+", "_");
        input = input.Trim('_', '-');

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
