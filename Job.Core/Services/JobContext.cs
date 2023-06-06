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
    
    private readonly TimeSpan _defaultTimeout = TimeSpan.FromSeconds(30);
    private readonly int _defaultMaxNrOfRetries = 5;
    private readonly TimeSpan _defaultMinBackoff = TimeSpan.FromSeconds(1);
    private readonly TimeSpan _defaultMaxBackoff =  TimeSpan.FromSeconds(3);
    
    public JobContext(ActorSystem actorSystem)
    {
        var masterActorProps = DependencyResolver
            .For(actorSystem)
            .Props<MasterActor<TIn,TOut>>();
        var type = GetGroupName();
        _masterActor = actorSystem.ActorOf(masterActorProps, $"master-{type}");;
    }

    public Guid CreateJob(TIn input,
        int? maxNrOfRetries = null,
        TimeSpan? minBackoff = null,
        TimeSpan? maxBackoff = null, 
        Guid? jobId = null,
        TimeSpan? timeout = null)
    {
        var currentTimeout = timeout ?? _defaultTimeout;
        var command = GetDoJobCommand(input, maxNrOfRetries, minBackoff, maxBackoff, jobId);
        _masterActor.Tell(command);
        return command.JobId;
    }

    public async Task<JobCommandResult> DoJobAsync(TIn input, 
        int? maxNrOfRetries = null, 
        TimeSpan? minBackoff = null,
        TimeSpan? maxBackoff = null, 
        Guid? jobId = null,
        TimeSpan? timeout = null)
    {
        var currentTimeout = timeout ?? _defaultTimeout;
        var command = GetDoJobCommand(input, maxNrOfRetries, minBackoff, maxBackoff, jobId);
        return await _masterActor.Ask<JobCommandResult>(command, currentTimeout);
    }

    public async Task<StopJobCommandResult> StopJobAsync(Guid jobId, TimeSpan? timeout = null)
    {
        var currentTimeout = timeout ?? _defaultTimeout;
        return await _masterActor.Ask<StopJobCommandResult>(
            new StopJobCommand(jobId, GetGroupName()), currentTimeout);
    }

    public async Task<IDictionary<Guid, ReplyWorkerInfo<TOut>>> GetAllJobsCurrentStatesAsync(long requestId,
        TimeSpan? timeout = null)
    {
        var currentTimeout = timeout ?? _defaultTimeout;
        var query = new RequestAllWorkersInfo(requestId, GetGroupName(), currentTimeout);
        RespondAllWorkersInfo<TOut> info = await _masterActor
            .Ask<RespondAllWorkersInfo<TOut>>(query, currentTimeout);
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
            minBackoff ?? _defaultMinBackoff,
            maxBackoff ?? _defaultMaxBackoff,
            maxNrOfRetries ?? _defaultMaxNrOfRetries);
    }
}