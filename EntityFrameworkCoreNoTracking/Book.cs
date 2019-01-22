namespace EntityFrameworkCoreNoTracking
{
    using System.ComponentModel.DataAnnotations.Schema;
    using Microsoft.EntityFrameworkCore;

    public class Book
    {
        [Column("id")]
        public int Id { get; set; }

        [Column("title")]
        public string Title { get; set; }

        [Column("author_id")]
        public int AuthorId { get; set; }

        public virtual Author Author { get; set; }

        internal static void OnModelCreating(ModelBuilder modelBuilder)
        {
            var e = modelBuilder.Entity<Book>();

            e.ToTable("books")
             .HasKey(x => x.Id);

            e.Property(x => x.Id)
             .HasColumnName("id");

            e.Property(x => x.Title)
             .HasColumnName("title")
             .IsRequired();

            e.HasOne(x => x.Author)
             .WithMany(a => a.Books)
             .HasForeignKey(x => x.AuthorId);
            e.Property(x => x.AuthorId)
             .HasColumnName("author_id");
        }
    }
}
