namespace EntityFrameworkCoreNoTracking
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    public static class Prep
    {
        private static int seed = Environment.TickCount;
        private static readonly ThreadLocal<Random> Rand = new ThreadLocal<Random>(() => new Random(Interlocked.Increment(ref seed)));

        public static async Task EnsureDbAsync(string connectionString, int bookCount, int authorCount)
        {
            using (var db = new MyDbContext(connectionString, true))
            {
                var created = await db.Database.EnsureCreatedAsync().ConfigureAwait(false);
                if (!created)
                {
                    return;
                }

                var authors = CreateAuthors(authorCount);
                await db.Authors.AddRangeAsync(authors).ConfigureAwait(false);
                await db.SaveChangesAsync().ConfigureAwait(false);

                var books = CreateBooks(bookCount, authors);
                await db.Books.AddRangeAsync(books).ConfigureAwait(false);
                await db.SaveChangesAsync().ConfigureAwait(false);
            }
        }

        private static Author[] CreateAuthors(int count)
        {
            var authors = new Author[count];
            for (var i = 0; i < count; i++)
            {
                authors[i] = new Author
                {
                    FirstName = "John",
                    LastName = $"Doe #{i+1}",
                };
            }

            return authors;
        }

        private static Book[] CreateBooks(int count, Author[] authors)
        {
            var books = new Book[count];
            for (var i = 0; i < count; i++)
            {
                books[i] = new Book
                {
                    Title = $"Book #{i+1}",
                    Author = authors[Rand.Value.Next(0, authors.Length)],
                };
            }

            return books;
        }
    }
}
