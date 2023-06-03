using Akka.Actor;
using Job.Core.Interfaces;
using Job.Core.Models;
using Job.Core.Theater.Workers.Messages;
using Microsoft.Extensions.DependencyInjection;

namespace Job.Core.Theater.Workers;

internal class WorkerActor<TData> : ReceiveActor 
    where TData : IJobData
{

    private Guid _actorId;
    private Type _groupId;
    
    private readonly IServiceScope _scope;
    
    private IJob<TData> _job;
    
    public WorkerActor(IServiceProvider serviceProvider)
    {
        _scope = serviceProvider.CreateScope();
        
        Receive<DoJobCommand>(DoJobCommandHandler);

        //Receive<Terminated>(DownloadActorSlaveTerminatedHandler);
    }

    private void DoJobCommandHandler(DoJobCommand command)
    {
        _actorId = command.JobId;
        _groupId = command.GroupType;
        _job.DoJob();
        Sender.Tell(new JobCommandResult(true, "Ok"));
    }
    
    protected override void PreStart()
    {
        _job = _scope.ServiceProvider.GetService<IJob<TData>>();
    }
    protected override void PostStop()
    {
        _scope.Dispose();
    }
}