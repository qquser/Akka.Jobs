using Job.Core;
using Job.Core.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Job.Tests;


 public class AkkaDiFixture<TIn, TOut, TJob>  : IDisposable
     where TIn : IJobInput
     where TOut : IJobResult 
     where TJob : class, IJob<TIn, TOut>
    {


        public AkkaDiFixture()
        {
            Services = new ServiceCollection()
                .AddLogging();
 
            Services.AddSingleton<ILoggerFactory, NullLoggerFactory>();
            
            Services.AddScoped<IJob<TIn, TOut>, TJob>();
            Services.AddJobContext();
            // </DiFixture>
            Provider = Services.BuildServiceProvider();
        }

        public IServiceCollection Services { get; private set; }

        public IServiceProvider Provider { get; private set; }

        public void Dispose()
        {
            Services = null;
            Provider = null;
        }
    }
  