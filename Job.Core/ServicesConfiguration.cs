using Akka.Actor;
using Akka.DependencyInjection;
using Job.Core.Theater.Master;
using Microsoft.Extensions.DependencyInjection;

namespace Job.Core;

public static class ServicesConfiguration
{
    public static void ConfigureJobServices(this IServiceCollection services)
    {
        //var serviceProvider = //
        var di = DependencyResolverSetup.Create(serviceProvider);

        var actorSystemSetup = BootstrapSetup.Create().And(di);
        var system = ActorSystem.Create("job-worker-system", actorSystemSetup);
        
        services.AddSingleton(_ => system);
        services.AddSingleton<IActorRef>(provider =>
        {
            var actorSystem = provider.GetService<ActorSystem>();
            return actorSystem.ActorOf(Props.Create<MasterActor>());
        });

    }
}