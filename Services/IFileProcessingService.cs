namespace FileRenamerProject.Services;

public interface IFileProcessingService
{
    Task ProcessFilesAsync(List<FileRecord> files, CancellationToken cancellationToken);
    Task ApplyRenameAsync(FileRecord file);
}
