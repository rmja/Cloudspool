using Api.Generators.ECMAScript6;
using Api.Infrastructure;
using AutoMapper;
using Cloudspool.AspNetCore.Authentication.ApiKey;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
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
            var redisConfiguration = ConfigurationOptions.Parse(Configuration.GetConnectionString("Redis"));

            services.AddSingleton(sp => ConnectionMultiplexer.Connect(redisConfiguration));
            services.AddControllers(options =>
            {
                options.InputFormatters.Insert(0, GetJsonPatchInputFormatter());
                options.Filters.Add(new AuthorizeFilter());
            })
                .ConfigureApplicationPartManager(options => options.FeatureProviders.Add(new ScanNestedControllersFeatureProvider(typeof(Startup).Assembly)))
                .ConfigureApiBehaviorOptions(options => options.SuppressInferBindingSourcesForParameters = true);

            services.AddDbContext<CloudspoolContext>(options => options.UseNpgsql(Configuration.GetConnectionString("Postgres")));

            services.AddAutoMapper(typeof(Startup).Assembly);
            services.AddMemoryCache();
            services.AddSingleton<ECMAScript6Generator>();

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = ApiKeyDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = ApiKeyDefaults.AuthenticationScheme;
            })
                .AddScheme<ApiKeyAuthenticationSchemeOptions, ApiKeyAuthenticationHandler>(ApiKeyDefaults.AuthenticationScheme, options => { });
            services.AddScoped<IApiKeyRepository, ApiKeyRepository>();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app
                    .UseDeveloperExceptionPage()
                    .UseCors(options => options.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
            }

            app
                .UsePathBase(Configuration["PathBase"])
                .Use((context, next) =>
                {
                    if (context.Request.Query.TryGetValue("project_key", out var key))
                    {
                        context.Request.Headers[HeaderNames.Authorization] = $"Bearer project:{key}";
                    }
                    return next();
                })
                .UseRouting()
                .UseAuthentication()
                .UseAuthorization()
                .UseEndpoints(endpoints =>
                {
                    endpoints.MapControllers();
                });

            Migrate(app);
        }

        public virtual void Migrate(IApplicationBuilder app)
        {
            using var scope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope();
            using var db = scope.ServiceProvider.GetRequiredService<CloudspoolContext>();
            db.Database.Migrate();
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
