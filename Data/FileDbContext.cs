
using Microsoft.EntityFrameworkCore;


namespace FileRenamerProject.Data;

public class FileDbContext : DbContext
{
    public DbSet<FileRecord>? Files { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite("Data Source=files.db");
    }
}

public class FileRecord
{
    public int Id { get; set; }
    public string? FileName { get; set; }
    public string? FilePath { get; set; }
    public string? SuggestedName { get; set; }
    public string? Status { get; set; }
}

