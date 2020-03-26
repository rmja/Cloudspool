using Cloudspool.AspNetCore.Authentication.ApiKey;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Net.Http.Headers;
using StackExchange.Redis;
using System;
using System.Threading.Tasks;

namespace Dispatcher
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
            services.AddApiClient(options =>
            {
                options.GetApiKeyAsync = sp =>
                {
                    var httpContextAccessor = sp.GetRequiredService<IHttpContextAccessor>();

                    var prefix = "Bearer ";
                    if (httpContextAccessor.HttpContext.Request.Headers.TryGetValue(HeaderNames.Authorization, out var headerValue) && headerValue[0].StartsWith(prefix))
                    {
                        return Task.FromResult(headerValue[0].Substring(prefix.Length));
                    }

                    return Task.FromResult<string>(null);
                };
            }).ConfigureHttpClient(client => client.BaseAddress = new Uri(Configuration["ApiBaseAddress"]));
            services.AddHttpContextAccessor();
            services.AddSignalR().AddStackExchangeRedis(options => options.Configuration = redisConfiguration);
            
            services.AddSingleton<PrintJobProcessor>().AddHostedService(x => x.GetRequiredService<PrintJobProcessor>());

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = ApiKeyDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = ApiKeyDefaults.AuthenticationScheme;
            })
                .AddScheme<ApiKeyAuthenticationSchemeOptions, ApiKeyAuthenticationHandler>(ApiKeyDefaults.AuthenticationScheme, options => { });
            services.AddScoped<IApiKeyRepository, SpoolerApiKeyRepository>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app
                .UsePathBase(Configuration["PathBase"])
                .UseRouting()
                .UseAuthentication()
                .UseAuthorization()
                .UseEndpoints(endpoints =>
                {
                    endpoints.MapHub<PrintingHub>("/Printing");
                    endpoints.MapGet("/", async context =>
                    {
                        await context.Response.WriteAsync("Hello World!");
                    });
                });
        }
    }
}
