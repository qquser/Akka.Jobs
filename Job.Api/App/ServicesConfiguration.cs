using Job.Api.Controllers;
using Job.Core.Interfaces;

namespace Job.Api.App;

public static class ServicesConfiguration
{
    public static void ConfigureServices(this IServiceCollection services)
    {
        services.AddScoped<IJob, ForEachJob>();
    }
}