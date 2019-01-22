namespace EntityFrameworkCoreNoTracking
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.EntityFrameworkCore;

    public sealed class MyDbContext : DbContext
    {
        private readonly string connectionString;
        private readonly bool enableTracking;

        public DbSet<Author> Authors { get; set; }

        public DbSet<Book> Books { get; set; }

        public MyDbContext(string connectionString, bool enableTracking)
        {
            this.connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
            this.enableTracking = enableTracking;
            ChangeTracker.AutoDetectChangesEnabled = enableTracking;
        }

        public async Task<Book> GetBookByIdAsync(int bookId)
        {
            if (enableTracking)
            {
                return await Books
                    .Include(b => b.Author)
                    .FirstOrDefaultAsync(x => x.Id == bookId)
                    .ConfigureAwait(false);
            }

            return await Books
                .AsNoTracking()
                .Include(b => b.Author)
                .FirstOrDefaultAsync(x => x.Id == bookId)
                .ConfigureAwait(false);
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (optionsBuilder.IsConfigured)
            {
                return;
            }

            optionsBuilder.UseNpgsql(connectionString, npgsqlOpts =>
            {
                npgsqlOpts.CommandTimeout(5); // secs
            });
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ForNpgsqlUseIdentityColumns();
            Book.OnModelCreating(modelBuilder);
            Author.OnModelCreating(modelBuilder);
        }
    }
}
