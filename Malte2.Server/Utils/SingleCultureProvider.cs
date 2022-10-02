using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Localization;

namespace Malte2.Server.Utils
{
    public class SingleCultureProvider : IRequestCultureProvider
    {
        public string CultureIdentifier { get; set; } = System.Globalization.CultureInfo.CurrentCulture.Name;

        public Task<ProviderCultureResult?> DetermineProviderCultureResult(HttpContext httpContext)
        {
            return Task.Run<ProviderCultureResult?>(() => new ProviderCultureResult(CultureIdentifier, CultureIdentifier));
        }
    }
}
