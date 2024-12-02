using System.Text;

namespace FileRenamerProject.Services;

public interface IFileLogger
{
    Task LogAsync(string message, LogLevel level = LogLevel.Information);
}

public class FileLogger : IFileLogger
{
    private readonly string _logFilePath;
    private readonly IConfiguration _configuration;
    private static readonly SemaphoreSlim _semaphore = new(1, 1);

    public FileLogger(IConfiguration configuration)
    {
        _configuration = configuration;
        var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
        _logFilePath = Path.Combine(baseDirectory, "logs", "app.log");
        
        // Ensure logs directory exists
        var logsDirectory = Path.GetDirectoryName(_logFilePath);
        if (!Directory.Exists(logsDirectory))
        {
            Directory.CreateDirectory(logsDirectory!);
        }
    }

    public async Task LogAsync(string message, LogLevel level = LogLevel.Information)
    {
        try
        {
            await _semaphore.WaitAsync();
            
            var logEntry = new StringBuilder()
                .Append(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"))
                .Append(" [").Append(level.ToString()).Append("] ")
                .Append(message)
                .AppendLine();

            await File.AppendAllTextAsync(_logFilePath, logEntry.ToString());
        }
        finally
        {
            _semaphore.Release();
        }
    }
}
