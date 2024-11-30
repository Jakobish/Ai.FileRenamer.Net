using Microsoft.EntityFrameworkCore;

namespace FileRenamerProject.Data;

public class FileDbContext : DbContext
{
    public DbSet<FileRecord> Files { get; set; } = null!;

    public FileDbContext(DbContextOptions<FileDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<FileRecord>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.FileName).IsRequired();
            entity.Property(e => e.FilePath).IsRequired();
            entity.Property(e => e.SuggestedName);
            entity.Property(e => e.Status).IsRequired();
        });
    }
}

public class FileRecord
{
    public int Id { get; set; }
    public string FileName { get; set; } = "";
    public string FilePath { get; set; } = "";
    public string SuggestedName { get; set; } = "";
    public string Status { get; set; } = "Pending";
}
