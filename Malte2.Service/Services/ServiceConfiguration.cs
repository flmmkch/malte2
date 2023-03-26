using Malte2.Database;

namespace Malte2.Services
{

    public static class ServiceConfiguration
    {
        public static void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<DatabaseContext>();
            services.AddScoped<AccountBookService>();
            services.AddScoped<AccountingEntryService>();
            services.AddScoped<AccountingCategoryService>();
            services.AddScoped<BoarderService>();
            services.AddScoped<BoardingRoomService>();
            services.AddScoped<OperationService>();
            services.AddScoped<OperatorService>();
            services.AddScoped<MealDayService>();
            services.AddScoped<RemissionService>();
        }
    }


}