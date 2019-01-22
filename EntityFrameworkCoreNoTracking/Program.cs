using System;
using System.Data;
using System.Diagnostics;
using System.Threading.Tasks;
using Npgsql;
using Dapper;
using System.Linq;
using Dapper.FluentMap;

namespace EntityFrameworkCoreNoTracking
{
    public static class Program
    {
        public static void Main()
        {
            RegisterDapperMappings();
            MainAsync().GetAwaiter().GetResult();
        }

        private static void RegisterDapperMappings()
        {
            FluentMapper.Initialize(config =>
            {
                config.AddMap(new AuthorMap());
                config.AddMap(new BookMap());
            });
        }

        private static async Task MainAsync()
        {
            const string cs = @"Host=localhost; Port=5432; Database=postgres; Username=dev; Password=dev; Timeout=5";
            const int bookCount = 50;
            const int authorCount = 10;
            const int repeatMeasurement = 5;

            await Prep.EnsureDbAsync(cs, bookCount, authorCount).ConfigureAwait(false);
            ForceGC();

            long npgsqlMs = 0;
            long dapperMs = 0;
            long trackOffMs = 0;
            long trackOnMs = 0;

            for (var round = 1; round <= repeatMeasurement; round++)
            {
                for (var bookId = 1; bookId <= bookCount; bookId++)
                {
                    var ms = await MeasureNpgsqlAsync(cs, bookId).ConfigureAwait(false);
                    npgsqlMs += ms;

                    ms = await MeasureDapperAsync(cs, bookId).ConfigureAwait(false);
                    dapperMs += ms;

                    ms = await MeasureEfAsync(false, cs, bookId).ConfigureAwait(false);
                    trackOffMs += ms;

                    ms = await MeasureEfAsync(true, cs, bookId).ConfigureAwait(false);
                    trackOnMs += ms;
                }

                ForceGC();
            }

            var reads = repeatMeasurement * bookCount;
            Console.WriteLine("{0} reads", reads);
            Console.WriteLine("npgsql (baseline):   {0} ms (avg {1} ms / read, 100%)", npgsqlMs, (double)npgsqlMs / reads);
            Console.WriteLine("dapper:              {0} ms (avg {1} ms / read, {2}%)", dapperMs, (double)dapperMs / reads, 100 * dapperMs / npgsqlMs);
            Console.WriteLine("EF without tracking: {0} ms (avg {1} ms / read, {2}%)", trackOffMs, (double)trackOffMs / reads, 100 * trackOffMs / npgsqlMs);
            Console.WriteLine("EF with tracking:    {0} ms (avg {1} ms / read, {2}%)", trackOnMs, (double)trackOnMs / reads, 100 * trackOnMs / npgsqlMs);

#if DEBUG
            if (Debugger.IsAttached)
            {
                Console.WriteLine("\r\npress any key to close the window...");
                Console.ReadKey();
            }
#endif
        }

        private static async Task<long> MeasureEfAsync(bool enableTracking, string connectionString, int bookId)
        {
            var sw = Stopwatch.StartNew();
            using (var db = new MyDbContext(connectionString, enableTracking))
            {
                var book = await db.GetBookByIdAsync(bookId).ConfigureAwait(false);
                Trace.Write(book.Title);
            }

            return sw.ElapsedMilliseconds;
        }

        private static async Task<long> MeasureNpgsqlAsync(string connectionString, int bookId)
        {
            var sw = Stopwatch.StartNew();
            const string sql = "select b.title, b.author_id, a.firstname, a.lastname from books as b inner join authors as a on b.author_id = a.id where b.id = {0}";
            using (var conn = new NpgsqlConnection(connectionString))
            {

                await conn.OpenAsync().ConfigureAwait(false);
                var q = string.Format(sql, bookId);
                using (var cmd = new NpgsqlCommand(q, conn))
                using (var reader = await cmd.ExecuteReaderAsync(CommandBehavior.SingleRow).ConfigureAwait(false))
                {
                    if (await reader.ReadAsync().ConfigureAwait(false))
                    {
                        var authorId = reader.GetInt32(1);
                        var book = new Book
                        {
                            Id = bookId,
                            Title = reader.GetString(0),
                            AuthorId = authorId,
                            Author = new Author
                            {
                                Id = authorId,
                                FirstName = reader.GetString(2),
                                LastName = reader.GetString(3),
                            }
                        };
                        Trace.Write(book.Title);
                    }
                }
            }

            return sw.ElapsedMilliseconds;
        }

        private static async Task<long> MeasureDapperAsync(string connectionString, int bookId)
        {
            var sw = Stopwatch.StartNew();
            using (var conn = new NpgsqlConnection(connectionString))
            {
                await conn.OpenAsync().ConfigureAwait(false);
                var book = await conn.QueryAsync<Book, Author, Book>(
                    @"select * from books as b inner join authors as a on b.author_id = a.id where b.id = @bookId",
                    (b, a) =>
                    {
                        b.Author = a;
                        return b;
                    },
                    new { bookId })
                    .ConfigureAwait(false);
                Trace.Write(book.First().Title);
            }

            return sw.ElapsedMilliseconds;
        }

        private static void ForceGC()
        {
            GC.Collect(2, GCCollectionMode.Forced);
            GC.WaitForPendingFinalizers();
        }
    }
}
