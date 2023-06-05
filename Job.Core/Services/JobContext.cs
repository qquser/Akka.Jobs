using System.Text.RegularExpressions;
using Akka.Actor;
using Akka.DependencyInjection;
using Job.Core.Interfaces;
using Job.Core.Models;
using Job.Core.Theater.ActorQueries.Messages;
using Job.Core.Theater.ActorQueries.Messages.States;
using Job.Core.Theater.Master;
using Job.Core.Theater.Master.Groups.Workers.Messages;

namespace Job.Core.Services;

internal class JobContext<TIn, TOut> : IJobContext<TIn, TOut> 
    where TIn : IJobInput
    where TOut : IJobResult
{
    private readonly IActorRef _masterActor;
    
    public JobContext(ActorSystem actorSystem)
    {
        var masterActorProps = DependencyResolver
            .For(actorSystem)
            .Props(typeof(MasterActor<,>)
                .MakeGenericType(typeof(TIn), typeof(TOut)));
        var type = GetGroupName();
        _masterActor = actorSystem.ActorOf(masterActorProps, $"master-{type}");;
    }

    public Guid CreateJob(TIn input,
        int? maxNrOfRetries = null, TimeSpan? minBackoff = null, TimeSpan? maxBackoff = null,  Guid? jobId = null)
    {
        var command = GetDoJobCommand(input, maxNrOfRetries, minBackoff, maxBackoff, jobId);
        _masterActor.Tell(command);
        return command.JobId;//TODO возвращать Guid только после создания актора воркера
    }

    public async Task<JobCommandResult> DoJobAsync(TIn input, 
        int? maxNrOfRetries = null, TimeSpan? minBackoff = null, TimeSpan? maxBackoff = null,  Guid? jobId = null)
    {
        var command = GetDoJobCommand(input, maxNrOfRetries, minBackoff, maxBackoff, jobId);
        return await _masterActor.Ask<JobCommandResult>(command);
    }

    public async Task<StopJobCommandResult> StopJobAsync(Guid jobId)
    {
        return await _masterActor.Ask<StopJobCommandResult>(
            new StopJobCommand(jobId, GetGroupName()));
    }

    public async Task<IDictionary<Guid, ReplyWorkerInfo<TOut>>> GetAllWorkersCurrentStateAsync(long requestId)
    {
        var query = new RequestAllWorkersInfo(requestId, GetGroupName(), TimeSpan.FromSeconds(30));
        RespondAllWorkersInfo<TOut> info = await _masterActor
            .Ask<RespondAllWorkersInfo<TOut>>(query);
        return info.WorkersData;
    }
    
    private string GetGroupName()
    {
        return Regex.Replace(GetType().ToString(), @"[^\w\d]", "_");;
    }
    
    private DoJobCommand<TIn> GetDoJobCommand(TIn input,
        int? maxNrOfRetries = null, TimeSpan? minBackoff = null, TimeSpan? maxBackoff = null,  Guid? jobId = null)
    {
        var id = jobId ?? Guid.NewGuid();
        return new DoJobCommand<TIn>(input,
            id,
            GetGroupName(),
            minBackoff ?? TimeSpan.FromSeconds(1),
            maxBackoff ?? TimeSpan.FromSeconds(3),
            maxNrOfRetries ?? 5);
    }
}