dotnet tool update --global dotnet-ef --version 8.20.0

dotnet ef migrations add InitialSqlite

dotnet ef migrations remove

dotnet ef database update -c ApplicationDbContext