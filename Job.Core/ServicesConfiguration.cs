using Akka.Actor;
using Akka.DependencyInjection;
using Job.Core.Interfaces;
using Job.Core.Services;
using Job.Core.Theater.Master;
using Microsoft.Extensions.DependencyInjection;

namespace Job.Core;

public static class ServicesConfiguration
{
    public static void ConfigureJobServices(this IServiceCollection services)
    {
        services.AddSingleton(provider =>
        {
            var di = DependencyResolverSetup.Create(provider);
            var actorSystemSetup = BootstrapSetup.Create().And(di);
            return ActorSystem.Create("job-worker-system", actorSystemSetup);
        });
        services.AddSingleton<IActorRef>(provider =>
        {
            var actorSystem = provider.GetService<ActorSystem>();
            return actorSystem.ActorOf(Props.Create<MasterActor>());
        });
        services.AddSingleton(typeof(IJobContext<>), typeof(JobContext<>));
    }
}