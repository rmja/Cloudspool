using Api.Tests.Infrastructure;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Linq;
using System.Threading;

namespace Api.Tests
{
    public class CustomWebApplicationFactory : WebApplicationFactory<TestStartup>
    {
        private static int _dbCounter = 0;
        private int? _assignedDbNumber;

        protected override IHostBuilder CreateHostBuilder()
        {
            return Host.CreateDefaultBuilder().ConfigureWebHostDefaults(webBuilder => webBuilder.UseStartup<TestStartup>());
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseSolutionRelativeContentRoot("src/Api");
            
            switch (Config.TestDatabaseProvider)
            {
                case DatabaseProvider.InMemory:
                    builder.ConfigureServices(services => WithInMemoryTestDatabase(services));
                    break;
                case DatabaseProvider.Npgsql:
                    builder.ConfigureServices(services => WithNpgsqlTestDatabase(services));
                    break;
            }
        }

        private void WithInMemoryTestDatabase(IServiceCollection services)
        {
            var descriptor = services.SingleOrDefault(x => x.ServiceType == typeof(DbContextOptions<CloudspoolContext>));

            if (descriptor is object)
            {
                // Remove default (real) implementation
                services.Remove(descriptor);
            }

            _assignedDbNumber ??= Interlocked.Increment(ref _dbCounter);
            services.AddDbContext<CloudspoolContext>(options =>
            {
                options.UseInMemoryDatabase($"TestDatabase-{_assignedDbNumber}");
            });
        }

        private void WithNpgsqlTestDatabase(IServiceCollection services)
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
}
