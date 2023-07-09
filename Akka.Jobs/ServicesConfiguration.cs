using Akka.Actor;
using Akka.DependencyInjection;
using Akka.Jobs.Interfaces;
using Akka.Jobs.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Akka.Jobs;

public static class ServicesConfiguration
{
    public static void AddJobContext(this IServiceCollection services, string config = "")
    {
        services.AddActorSystemSingleton(config);
        services.AddJobContextSingleton();
    }
}

internal static class InternalServicesConfiguration
{
    public static void AddActorSystemSingleton(this IServiceCollection services, string config = "")
    {
        services.AddSingleton(provider =>
        {
            var di = DependencyResolverSetup.Create(provider);
            var actorSystemSetup = BootstrapSetup.Create()
                .WithConfig(config)
                .And(di);
            return ActorSystem.Create("job-worker-system", actorSystemSetup);
        });
    }
    public static void AddJobContextSingleton(this IServiceCollection services)
    {
        services.AddSingleton(typeof(IJobContext<,>), typeof(JobContext<,>));
    }
}