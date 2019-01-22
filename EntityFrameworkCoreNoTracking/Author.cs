namespace EntityFrameworkCoreNoTracking
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations.Schema;
    using Microsoft.EntityFrameworkCore;

    public class Author
    {
        [Column("id")]
        public int Id { get; set; }

        [Column("firstname")]
        public string FirstName { get; set; }

        [Column("lastname")]
        public string LastName { get; set; }

        public virtual ICollection<Book> Books { get; set; }

        internal static void OnModelCreating(ModelBuilder modelBuilder)
        {
            var e = modelBuilder.Entity<Author>();

            e.ToTable("authors")
             .HasKey(x => x.Id);

            e.Property(x => x.Id)
             .HasColumnName("id");

            e.Property(x => x.FirstName)
             .HasColumnName("firstname")
             .IsRequired();

            e.Property(x => x.LastName)
             .HasColumnName("lastname")
             .IsRequired();
        }
    }
}
