using System.Net.Http.Json;
using System.Text;
using System.Text.RegularExpressions;

using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;

namespace FileRenamerProject.Services;

public class PdfService : IPdfService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;

    public PdfService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _configuration = configuration;
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

    // Placeholder for the hypothetical Gemini API call
    private async Task<string> GetGeminiSuggestionAsync(string content, string apiKey)
    {
        // Replace this with the actual Gemini API call
        // This is a placeholder and would need to be implemented using the Gemini API client library
        // Assume this method handles authentication and response parsing
        await Task.Delay(1000); // Simulate API call delay
        return "Suggested_Filename"; 
    }

    // Placeholder for Gemini specific exception
    public class GeminiApiException : Exception {
        public GeminiApiException(string message) : base(message) { }
    } 
}
