using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.EntityFrameworkCore.Storage;

namespace ef_core_query_sql
{
    class Program
    {
        static void Main(string[] args)
        {
            using (var appDbContext = new AppDbContext())
            {
                appDbContext.Database.EnsureDeleted();
                appDbContext.Database.EnsureCreated();
                Console.WriteLine("The database has been reset.");
                Console.WriteLine(appDbContext.Users.Include(u => u.Car).Where(u => u.Car != null).ToSql());
            }
        }
    }

    public class AppDbContext: DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Car> Cars { get; set; }
        public DbSet<Trip> Trips { get; set; }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer($@"Server=(localdb)\{nameof(ef_core_query_sql)};Database={nameof(ef_core_query_sql)};");
        }
    }

    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public Car Car { get; set; }
        public int CarId { get; set; }
    }

    public class Car
    {
        public int Id { get; set; }
        public string Model { get; set; }
        public string Make { get; set; }
        public ICollection<Trip> Trips { get; set; }
    }

    public class Trip
    {
        public int Id { get; set; }
        public DateTime DateAndTime { get; set; }
        public int DistanceInKilometers { get; set; }
        public Car Car { get; set; }
        public int CarId { get; set; }
    }
    public static class IQueryableExtensions
    {
        private static readonly TypeInfo QueryCompilerTypeInfo = typeof(QueryCompiler).GetTypeInfo();

        private static readonly FieldInfo QueryCompilerField = typeof(EntityQueryProvider).GetTypeInfo().DeclaredFields.First(x => x.Name == "_queryCompiler");

        private static readonly FieldInfo QueryModelGeneratorField = QueryCompilerTypeInfo.DeclaredFields.First(x => x.Name == "_queryModelGenerator");

        private static readonly FieldInfo DataBaseField = QueryCompilerTypeInfo.DeclaredFields.Single(x => x.Name == "_database");

        private static readonly PropertyInfo DatabaseDependenciesField = typeof(Database).GetTypeInfo().DeclaredProperties.Single(x => x.Name == "Dependencies");

        public static string ToSql<TEntity>(this IQueryable<TEntity> query) where TEntity : class
        {
            var queryCompiler = (QueryCompiler)QueryCompilerField.GetValue(query.Provider);
            var modelGenerator = (QueryModelGenerator)QueryModelGeneratorField.GetValue(queryCompiler);
            var queryModel = modelGenerator.ParseQuery(query.Expression);
            var database = (IDatabase)DataBaseField.GetValue(queryCompiler);
            var databaseDependencies = (DatabaseDependencies)DatabaseDependenciesField.GetValue(database);
            var queryCompilationContext = databaseDependencies.QueryCompilationContextFactory.Create(false);
            var modelVisitor = (RelationalQueryModelVisitor)queryCompilationContext.CreateQueryModelVisitor();
            modelVisitor.CreateQueryExecutor<TEntity>(queryModel);
            var sql = modelVisitor.Queries.First().ToString();

            return sql;
        }
    }
}
