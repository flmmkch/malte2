using Malte2.Database;

namespace Malte2.Services
{

    public static class ServiceConfiguration
    {
        public static void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<DatabaseContext>();
            services.AddScoped<OperatorService>();
        }
    }


}