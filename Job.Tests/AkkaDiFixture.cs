using Akka.Jobs;
using Akka.Jobs.Interfaces;
using Microsoft.Extensions.DependencyInjection;
// ReSharper disable ClassNeverInstantiated.Global

namespace Job.Tests;

public sealed class AkkaDiFixture<TIn, TOut, TJob>  : IDisposable
     where TIn : IJobInput
     where TOut : IJobResult 
     where TJob : class, IJob<TIn, TOut>
{
    public AkkaDiFixture()
    {
        Services = new ServiceCollection();
        
        Services.AddScoped<IJob<TIn, TOut>, TJob>();
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