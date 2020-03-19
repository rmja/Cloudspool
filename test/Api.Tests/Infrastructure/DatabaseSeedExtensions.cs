using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Respawn;
using System.Data;

namespace Api.Tests
{
    public static class DatabaseSeedExtensions
    {
        private static readonly Checkpoint _postgresCheckpoint = new Checkpoint
        {
            SchemasToInclude = new[] { "public" },
            DbAdapter = DbAdapter.Postgres
        };

        public static WebApplicationFactory<TStartup> WithPopulatedSeedData<TStartup>(this WebApplicationFactory<TStartup> factory) where TStartup : class =>
            factory.WithWebHostBuilder(builder => builder.ConfigureServices(services => services.PopulateSeedData()));

        // See https://docs.microsoft.com/en-us/aspnet/core/test/integration-tests?view=aspnetcore-3.1
        private static void PopulateSeedData(this IServiceCollection services)
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

                    _postgresCheckpoint.Reset(connection).GetAwaiter().GetResult();

                    if (wasClosed)
                    {
                        connection.Close();
                    }

                    SeedData.Populate(db);
                }
            }
        }
    }
}
