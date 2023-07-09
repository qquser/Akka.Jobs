using System.Runtime.CompilerServices;
using Akka.Actor;
using Akka.Jobs.Interfaces;
using Akka.Jobs.Models;
using Akka.Jobs.Theater.ActorQueries.Messages.States;
using Akka.Jobs.Theater.Master.Groups.Workers.Messages;
using Microsoft.Extensions.DependencyInjection;

[assembly:InternalsVisibleTo("Job.Tests")]
namespace Akka.Jobs.Theater.Master.Groups.Workers;

internal sealed class WorkerActor<TIn, TOut> : ReceiveActor
    where TIn : IJobInput
    where TOut : IJobResult
{
    private string? _jobId;

    private readonly IServiceScope _scope;
    private readonly IActorRef _self;
    
    private IJob<TIn, TOut>? _job;

    private readonly string _jobNullError = 
        "Error, _job cannot be null. Interface IJob<TIn, TOut> has not been registered.";

    public WorkerActor(IServiceProvider serviceProvider)
    {
        _self = Self;
        _scope = serviceProvider.CreateScope();
        
        //Commands
        Receive<WorkerDoJobCommand<TIn>>((msg) =>
        {
            WorkerDoJobCommandHandlerAsync(msg).PipeTo(_self);
        });
        
        //Queries
        Receive<ReadWorkerInfoCommand>(ReadWorkerInfoCommandHandler);
        
        //Internal
        Receive<Status.Failure>(Failed);
        
        Context.Parent.Tell(new GiveMeWorkerDoJobCommand());
    }
    
    private void Failed(Status.Failure msg)
    {
        throw msg.Cause ?? throw new Exception("Unknown error, msg.Cause == null");
    }
    
    private void ReadWorkerInfoCommandHandler(ReadWorkerInfoCommand command)
    {
        if (_job == null || string.IsNullOrWhiteSpace(_jobId))
        {
            var error = new RespondWorkerInfo<TOut>(false, command.RequestId, _jobNullError);
            Sender.Tell(error);
            return;
        }

        var currentState = _job.GetCurrentState(_jobId);
        var result = new RespondWorkerInfo<TOut>(command.RequestId, currentState);
        Sender.Tell(result);
    }
    
    private async Task WorkerDoJobCommandHandlerAsync(WorkerDoJobCommand<TIn> command)
    {
        if (_job != null)
        {
            _jobId = command.JobId;
            Context.Parent.Tell(new TrySaveWorkerActorRefCommand(Self, _jobId, command.DoJobCommandSender));
        
            if(command.IsCreateCommand)
                command.DoJobCommandSender.Tell(new JobCreatedCommandResult(true, "", _jobId));
        
            var token = command.CancellationTokenSource.Token;
            var jobResult = await _job.DoAsync(command.JobInput, token);
        
            if(!command.IsCreateCommand && !token.IsCancellationRequested)
                command.DoJobCommandSender.Tell(new JobDoneCommandResult(jobResult, "Ok", command.JobId));
        }
        else
        {
            command.DoJobCommandSender.Tell(command.IsCreateCommand 
                ? new JobCreatedCommandResult(false, _jobNullError, command.JobId)
                : new JobDoneCommandResult(false, _jobNullError, command.JobId));
        }

        _self.Tell(PoisonPill.Instance);
    }

    protected override void PreStart()
    {
        _job = _scope.ServiceProvider.GetService<IJob<TIn, TOut>>();
    }
    
    protected override void PostStop()
    {
        _scope.Dispose();
    }
}