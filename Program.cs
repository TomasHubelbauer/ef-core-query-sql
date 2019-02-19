using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
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
                var query = appDbContext.Users.Include(u => u.Car).Where(u => u.Car != null);
                var expression = query.Expression;
                Console.WriteLine(expression);
                var expression2 = GetExpression(() => appDbContext.Users.Include(u => u.Car).First()) as LambdaExpression;
                Console.WriteLine(expression2.Body);

                var bindingFlags = BindingFlags.NonPublic | BindingFlags.Instance;
                var queryCompiler = query.Provider.GetType().GetField("_queryCompiler", bindingFlags).GetValue(query.Provider) as QueryCompiler;
                var modelGenerator = queryCompiler.GetType().GetField("_queryModelGenerator", bindingFlags).GetValue(queryCompiler) as QueryModelGenerator;
                var database = queryCompiler.GetType().GetField("_database", bindingFlags).GetValue(queryCompiler) as IDatabase;
                var databaseDependencies = (DatabaseDependencies)typeof(Database).GetTypeInfo().DeclaredProperties.Single(x => x.Name == "Dependencies").GetValue(database);
                var queryCompilationContext = databaseDependencies.QueryCompilationContextFactory.Create(false);
                var modelVisitor = (RelationalQueryModelVisitor)queryCompilationContext.CreateQueryModelVisitor();
                modelVisitor.CreateQueryExecutor<User>(modelGenerator.ParseQuery(expression));
                foreach (var sql in modelVisitor.Queries) {
                    Console.WriteLine("Query:");
                    Console.WriteLine(sql);
                }
            }
        }

        private static object GetPrivateField<TSource>(TSource source, string name)
        {
            return source.GetType().GetField(name, BindingFlags.NonPublic | BindingFlags.Instance).GetValue(source);
        }

        static Expression GetExpression<T>(Expression<Func<T>> expression)
        {
            return expression;
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
}
