namespace FileRenamerProject.Services;

public interface IFileProcessingService
{
    Task ProcessFilesAsync(List<FileRenamerProject.Data.FileRecord> files, CancellationToken cancellationToken);
    Task ApplyRenameAsync(FileRenamerProject.Data.FileRecord file);
}
