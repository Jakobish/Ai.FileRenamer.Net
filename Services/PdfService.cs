using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using iText.Kernel.Pdf.Canvas.Parser.Listener;

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
                textBuilder.AppendLine(text);
            }

            return textBuilder.ToString();
        }
        catch (Exception ex)
        {
            throw new Exception($"Error extracting text from PDF: {ex.Message}", ex);
        }
    }

    public async Task<string> GetSuggestedNameFromAIAsync(string fileName, string content)
    {
        try
        {
            var apiKey = _configuration["OpenAIApiKey"];
            if (string.IsNullOrEmpty(apiKey))
            {
                throw new InvalidOperationException("OpenAI API key not configured");
            }

            _httpClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", apiKey);

            // Take only the first 1000 characters of content to avoid token limits
            var truncatedContent = content.Length > 1000 ? content.Substring(0, 1000) : content;

            var requestData = new
            {
                model = "gpt-3.5-turbo",
                messages = new[]
                {
                    new
                    {
                        role = "system",
                        content = "You are a helpful assistant that suggests concise, descriptive filenames based on PDF content. The filename should be clear, descriptive, and follow these rules: use underscores instead of spaces, be less than 50 characters, include only letters, numbers, and underscores, and end with .pdf"
                    },
                    new
                    {
                        role = "user",
                        content = $"Please suggest a filename for a PDF with the following content:\n\n{truncatedContent}\n\nCurrent filename is: {fileName}"
                    }
                }
            };

            var response = await _httpClient.PostAsJsonAsync(
                "https://api.openai.com/v1/chat/completions",
                requestData
            );

            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException($"OpenAI request failed: {response.StatusCode}");
            }

            var result = await response.Content.ReadFromJsonAsync<OpenAIResponse>();
            if (result?.Choices == null || result.Choices.Length == 0)
            {
                throw new InvalidOperationException("No suggestion received from OpenAI");
            }

            var suggestedName = result.Choices[0].Message.Content.Trim();

            // Clean up the suggested name
            suggestedName = Regex.Replace(suggestedName, @"[^\w\d_]", "_");
            suggestedName = Regex.Replace(suggestedName, @"_+", "_");
            suggestedName = suggestedName.TrimEnd('_');

            // Remove .pdf if it was included
            suggestedName = suggestedName.Replace(".pdf", "");

            if (suggestedName.Length > 50)
            {
                suggestedName = suggestedName.Substring(0, 50);
            }

            return suggestedName;
        }
        catch (Exception ex)
        {
            throw new Exception($"Error getting AI suggestion: {ex.Message}", ex);
        }
    }

    private class OpenAIResponse
    {
        public Choice[] Choices { get; set; } = Array.Empty<Choice>();
    }

    private class Choice
    {
        public Message Message { get; set; } = new();
    }

    private class Message
    {
        public string Content { get; set; } = "";
    }
}
