namespace EntityFrameworkCoreNoTracking
{
    using Dapper.FluentMap.Mapping;

    public class BookMap : EntityMap<Book>
    {
        public BookMap()
        {
            Map(x => x.Id)
                .ToColumn("id");

            Map(x => x.Title)
                .ToColumn("title");

            Map(x => x.AuthorId)
                .ToColumn("author_id");
        }
    }
}
