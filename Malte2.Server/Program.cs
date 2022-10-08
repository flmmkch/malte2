using System.Globalization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllersWithViews();

Malte2.Services.ServiceConfiguration.ConfigureServices(builder.Services);

var app = builder.Build();

// localization
// set to French
var requestOpt = new RequestLocalizationOptions();
requestOpt.SupportedCultures = new List<CultureInfo>
{
    new CultureInfo("fr-FR")
};
requestOpt.SupportedUICultures = new List<CultureInfo>
{
    new CultureInfo("fr-FR")
};
requestOpt.RequestCultureProviders.Clear();
requestOpt.RequestCultureProviders.Add(new Malte2.Server.Utils.SingleCultureProvider() { CultureIdentifier = "fr-FR" });

app.UseRequestLocalization(requestOpt);

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
}

app.UseStaticFiles();
app.UseRouting();

app.MapControllerRoute(
    name: "default",
    pattern: "api/{controller}/{action=Index}/{id?}");

string locale = "fr";

app.MapFallbackToFile($"{locale}/index.html");

app.Run();
