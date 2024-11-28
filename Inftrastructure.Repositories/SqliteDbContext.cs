using Inftrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

public class SqliteDbContext : DbContext
{
    public DbSet<CommitRecord> Commits { get; set; }

    public SqliteDbContext(DbContextOptions<SqliteDbContext> options) : base(options)
    {
        Database.EnsureCreated();
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.ConfigureWarnings(warnings => warnings.Ignore(RelationalEventId.PendingModelChangesWarning));
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CommitRecord>()
            .HasKey(c => c.Sha);

        modelBuilder.Entity<CommitRecord>()
            .HasIndex(c => new { c.Owner, c.Repo, c.Sha })
            .IsUnique();
    }
}
