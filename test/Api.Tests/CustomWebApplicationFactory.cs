using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Respawn;
using System;
using System.Data;
using System.Linq;
using System.Threading;

namespace Api.Tests
{
    public class CustomWebApplicationFactory : WebApplicationFactory<Startup>
    {
        public static Checkpoint Checkpoint = new Checkpoint
        {
            SchemasToInclude = new[] { "public" },
            DbAdapter = DbAdapter.Postgres
        };

        private static int _dbCounter = 0;
        private int? _assignedDbNumber;

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseSolutionRelativeContentRoot("src/Api");
            
            //.ConfigureServices(services => UseInMemoryDatabase(services));
            builder.ConfigureServices(services => UseNpgsqlDatabase(services));
        }

        private static void UseInMemoryDatabase(IServiceCollection services)
        {
            var descriptor = services.SingleOrDefault(x => x.ServiceType == typeof(DbContextOptions<CloudspoolContext>));

            if (descriptor is object)
            {
                // Remove default (real) implementation
                services.Remove(descriptor);
            }

            services.AddDbContext<CloudspoolContext>(options =>
            {
                options.UseInMemoryDatabase(Guid.NewGuid().ToString());
            });
        }

        private void UseNpgsqlDatabase(IServiceCollection services)
        {
            var descriptor = services.SingleOrDefault(x => x.ServiceType == typeof(DbContextOptions<CloudspoolContext>));

            if (descriptor is object)
            {
                // Remove default (real) implementation
                services.Remove(descriptor);
            }

            _assignedDbNumber ??= Interlocked.Increment(ref _dbCounter);
            var connectionString = string.Format("Host=localhost;Database=cloudspool-test-{0};Username=cloudspool;Password=cloudspool", _assignedDbNumber);

            services.AddDbContext<CloudspoolContext>(options =>
            {
                options.UseNpgsql(connectionString);
            });

            var serviceProvider = services.BuildServiceProvider();

            using (var scope = serviceProvider.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<CloudspoolContext>();
                db.Database.EnsureCreated();
            }
        }
    }

    public static class InMemoryDatabaseServiceCollectionExtensions
    {
        public static WebApplicationFactory<TStartup> WithPopulatedSeedData<TStartup>(this WebApplicationFactory<TStartup> factory) where TStartup : class =>
            factory.WithWebHostBuilder(builder => builder.ConfigureServices(services => services.PopulateSeedData()));

        // See https://docs.microsoft.com/en-us/aspnet/core/test/integration-tests?view=aspnetcore-3.1
        public static IServiceCollection PopulateSeedData(this IServiceCollection services)
        {
            var serviceProvider = services.BuildServiceProvider();

            using (var scope = serviceProvider.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<CloudspoolContext>();

                if (db.Database.IsInMemory())
                {
                    // Ensure a clean slate
                    db.Database.EnsureDeleted();
                    db.Database.EnsureCreated();

                    // Seed the database with test data
                    SeedData.Populate(db);
                }
                else if (db.Database.IsNpgsql())
                {
                    // Ensure a clean slate
                    var connection = db.Database.GetDbConnection();
                    var wasClosed = connection.State == ConnectionState.Closed;

                    if (wasClosed)
                    {
                        connection.Open();
                    }

                    CustomWebApplicationFactory.Checkpoint.Reset(connection).GetAwaiter().GetResult();

                    if (wasClosed)
                    {
                        connection.Close();
                    }

                    // Seed the database with test data
                    SeedData.Populate(db);
                }
            }

            return services;
        }
    }
}
