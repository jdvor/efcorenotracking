namespace EntityFrameworkCoreNoTracking
{
    using Dapper.FluentMap.Mapping;

    public class AuthorMap : EntityMap<Author>
    {
        public AuthorMap()
        {
            Map(x => x.Id)
                .ToColumn("id");

            Map(x => x.FirstName)
                .ToColumn("firstname");

            Map(x => x.LastName)
                .ToColumn("lastname");
        }
    }
}
