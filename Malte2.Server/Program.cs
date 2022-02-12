using Malte2.Database;
using Malte2.Application;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllersWithViews();

// database service
string connectionString = builder.Configuration.GetConnectionString("Malte2");
DatabaseContext databaseContext = new DatabaseContext(connectionString);
DatabaseUpdater.UpdateDatabase(databaseContext);

builder.Services.AddSingleton(databaseContext);

var app = builder.Build();

// log database info
app.Logger.LogInformation(LogConstants.DATABASE_INITIALIZATION, "Connection string: {0}", connectionString);
if (databaseContext.HasForeignKeySupport() != true) {
    app.Logger.LogError(LogConstants.DATABASE_INITIALIZATION, "Foreign keys support: {0}", databaseContext.HasForeignKeySupport());
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
}

app.UseStaticFiles();
app.UseRouting();

app.MapControllerRoute(
    name: "default",
    pattern: "api/{controller}/{action=Index}/{id?}");

app.MapFallbackToFile("index.html");

app.Run();
