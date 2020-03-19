using Api.Generators.ECMAScript6;
using Api.Infrastructure;
using AutoMapper;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using System.Linq;

namespace Api
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            var redisConfiguration = ConfigurationOptions.Parse("redis");

            services.AddSingleton(sp => ConnectionMultiplexer.Connect(redisConfiguration));
            services.AddControllers(options => options.InputFormatters.Insert(0, GetJsonPatchInputFormatter()))
                .ConfigureApplicationPartManager(options => options.FeatureProviders.Add(new ScanNestedControllersFeatureProvider(typeof(Startup).Assembly)))
                .ConfigureApiBehaviorOptions(options => options.SuppressInferBindingSourcesForParameters = true);

            services.AddDbContext<CloudspoolContext>(options => options.UseNpgsql("Host=localhost;Database=cloudspool;Username=cloudspool;Password=cloudspool"));
            //services.AddDbContext<CloudspoolContext>(options => options.UseInMemoryDatabase("Cloudspool"));

            services.AddAutoMapper(typeof(Startup).Assembly);
            services.AddMemoryCache();
            services.AddSingleton<ECMAScript6Generator>();
            services.AddTransient<ProjectKeyAuthorizationMiddleware>();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseMiddleware<ProjectKeyAuthorizationMiddleware>();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }

        private static NewtonsoftJsonPatchInputFormatter GetJsonPatchInputFormatter()
        {
            var builder = new ServiceCollection()
                .AddLogging()
                .AddMvc()
                .AddNewtonsoftJson()
                .Services.BuildServiceProvider();

            return builder
                .GetRequiredService<IOptions<MvcOptions>>()
                .Value
                .InputFormatters
                .OfType<NewtonsoftJsonPatchInputFormatter>()
                .First();
        }
    }
}
