using FileRenamerProject.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.JSInterop;

namespace FileRenamerProject.Services;

public class FileProcessingService : IFileProcessingService
{
    private readonly FileDbContext _dbContext;
    private readonly IPdfService _pdfService;
    private readonly IJSRuntime _jsRuntime;

    public FileProcessingService(FileDbContext dbContext, IPdfService pdfService, IJSRuntime jsRuntime)
    {
        _dbContext = dbContext;
        _pdfService = pdfService;
        _jsRuntime = jsRuntime;
    }

    public async Task ProcessFilesAsync(List<FileRecord> files, CancellationToken cancellationToken)
    {
        const int batchSize = 3;
        var pendingFiles = files.Where(f => f.Status == "Pending").ToList();

        for (int i = 0; i < pendingFiles.Count; i += batchSize)
        {
            if (cancellationToken.IsCancellationRequested)
                break;

            var batch = pendingFiles.Skip(i).Take(batchSize).ToList();
            var tasks = batch.Select(file => ProcessSingleFileAsync(file, cancellationToken));
            await Task.WhenAll(tasks);

            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }

    private async Task ProcessSingleFileAsync(FileRecord file, CancellationToken cancellationToken)
    {
        try
        {
            file.Status = "Processing";

            var fileBytes = await _jsRuntime.InvokeAsync<byte[]>("getFileBytes", file.FilePath);
            cancellationToken.ThrowIfCancellationRequested();

            if (fileBytes == null || fileBytes.Length == 0)
            {
                throw new InvalidOperationException("File is empty or could not be read");
            }

            var content = await _pdfService.ExtractTextFromPdfAsync(fileBytes);
            cancellationToken.ThrowIfCancellationRequested();

            if (string.IsNullOrWhiteSpace(content))
            {
                throw new InvalidOperationException("No text could be extracted from the PDF");
            }

            var suggestedName = await _pdfService.GetSuggestedNameFromAIAsync(file.FileName, content);
            cancellationToken.ThrowIfCancellationRequested();

            if (string.IsNullOrWhiteSpace(suggestedName))
            {
                throw new InvalidOperationException("Could not generate a suggested name");
            }

            suggestedName = string.Join("_", suggestedName.Split(Path.GetInvalidFileNameChars()));
            file.SuggestedName = suggestedName;
            file.Status = "Completed";

            var existingFile = _dbContext.Files.FirstOrDefault(f => f.FilePath == file.FilePath);
            if (existingFile != null)
            {
                _dbContext.Entry(existingFile).CurrentValues.SetValues(file);
            }
            else
            {
                _dbContext.Files.Add(file);
            }
        }
        catch (Exception ex)
        {
            file.Status = "Error";
            file.SuggestedName = ex.Message;
        }
    }

    public async Task ApplyRenameAsync(FileRecord file)
    {
        if (file == null) return;

        try
        {
            var newFileName = Path.GetFileNameWithoutExtension(file.SuggestedName) + ".pdf";
            file.FileName = newFileName;
            file.Status = "Renamed";
            await _dbContext.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            file.Status = "Error";
            throw new InvalidOperationException($"Error renaming file: {ex.Message}", ex);
        }
    }


    public Task ApplyRenameAsync(FileRecord file)
    {
        throw new NotImplementedException();
    }
}
