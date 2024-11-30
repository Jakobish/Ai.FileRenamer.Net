namespace FileRenamerProject.Services;

public interface IPdfService
{
    Task<string> ExtractTextFromPdfAsync(byte[] pdfBytes);
    Task<string> GetSuggestedNameFromAIAsync(string fileName, string content);
}
