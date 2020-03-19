using Intercom;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using StackExchange.Redis;
using System;

namespace Dispatcher
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            var redisConfiguration = ConfigurationOptions.Parse("redis");

            services.AddSingleton(sp => ConnectionMultiplexer.Connect(redisConfiguration));
            services.AddApiClient().ConfigureHttpClient(client => client.BaseAddress = new Uri("https://localhost:51331"));
            services.AddSignalR().AddStackExchangeRedis(options => options.Configuration = redisConfiguration);
            
            services.AddSingleton<PrintJobProcessor>().AddHostedService(x => x.GetRequiredService<PrintJobProcessor>());
            services.AddSingleton<SpoolerKeyAuthorizationMiddleware>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseMiddleware<SpoolerKeyAuthorizationMiddleware>();

            app.UseRouting();

            app.UseEndpoints(endpoints =>
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
