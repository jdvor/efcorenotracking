# EF Core no-tracking performance issue

1) build solution
2) run `docker-compose up` to spin up local PostgreSQL database
3) run `dotnet EntityFrameworkCoreNoTracking/bin/Release/netcoreapp2.2/EntityFrameworkCoreNoTracking.dll`.
4) clean up `docker-compose down`