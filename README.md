# EF Core Query SQL

This is a repository dedicated to testing out a reflection-based snippet for printing SQL generated from
a EF Core query LINQ expression.

The snippet code can be found in multiple places with comments worth following for updated versions:

- [Stack Overflow question](https://stackoverflow.com/q/37527783/2715716)
- [GitHub gist](https://gist.github.com/rionmonster/2c59f449e67edf8cd6164e9fe66c545a)
- [Blog post 1](http://rion.io/2016/10/19/accessing-entity-framework-core-queries-behind-the-scenes-in-asp-net-core/)
- [Blog post 2](https://weblogs.asp.net/ricardoperes/implementing-missing-features-in-entity-framework-core-part-5-getting-the-sql-for-a-query)

Let's create the test application and include the EF code NuGet packages:

```powershell
dotnet new console
dotnet add package Microsoft.EntityFrameworkCore
dotnet add package Microsoft.EntityFrameworkCore.SqlServer
```

For the classes see, `AppDbContext`, `User`, `Car` and `Trip` in code.

```csharp
Console.WriteLine(appDbContext.Users.Include(u => u.Car).Where(u => u.Car != null).ToSql());
```

```sql
SELECT [u].[Id], [u].[CarId], [u].[Name], [u.Car].[Id], [u.Car].[Make], [u.Car].[Model]
FROM [Users] AS [u]
INNER JOIN [Cars] AS [u.Car] ON [u].[CarId] = [u.Car].[Id]
WHERE [u].[CarId] IS NOT NULL
```

## To-Do

### Turn this into a hosted app (Azure Function?) and offer it as a tool for the dev community

### https://github.com/aspnet/EntityFrameworkCore/wiki/Getting-and-Building-the-Code and debug passing a `First` expression

### Mention the detailed logging option which shows the executed SQL statements
