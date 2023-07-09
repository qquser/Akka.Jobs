using Akka.Jobs;
using Akka.Jobs.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Job.Tests;

public sealed class AkkaDiFixtureWithoutJob<TIn, TOut, TJob>  : IDisposable
    where TIn : IJobInput
    where TOut : IJobResult 
    where TJob : class, IJob<TIn, TOut>
{
    public AkkaDiFixtureWithoutJob()
    {
        Services = new ServiceCollection();
        
        Services.AddJobContext();
        // </DiFixture>
        Provider = Services.BuildServiceProvider();
    }

    public IServiceCollection? Services { get; private set; }
    public IServiceProvider? Provider { get; private set; }

    public void Dispose()
    {
        Services = null;
        Provider = null;
    }
}