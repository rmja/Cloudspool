using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;

namespace Api.Tests.Infrastructure
{
    public class TestStartup : Startup
    {
        public TestStartup(IConfiguration configuration) : base(configuration)
        {
        }

        public override void Migrate(IApplicationBuilder app)
        {
        }
    }
}
