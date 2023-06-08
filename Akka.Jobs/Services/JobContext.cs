using System.Text.RegularExpressions;
using Akka.Actor;
using Akka.DependencyInjection;
using Akka.Jobs.Interfaces;
using Akka.Jobs.Models;
using Akka.Jobs.Theater.ActorQueries.Messages;
using Akka.Jobs.Theater.ActorQueries.Messages.States;
using Akka.Jobs.Theater.Master;
using Akka.Jobs.Theater.Master.Groups.Workers.Messages;

namespace Akka.Jobs.Services;

internal class JobContext<TIn, TOut> : IJobContext<TIn, TOut> 
    where TIn : IJobInput
    where TOut : IJobResult
{
    private readonly IActorRef _masterActor;
    
    private readonly TimeSpan _defaultTimeout = TimeSpan.FromSeconds(120);
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

    public async Task<JobCreatedCommandResult> CreateJobAsync(TIn input,
        int? maxNrOfRetries = null,
        TimeSpan? minBackoff = null,
        TimeSpan? maxBackoff = null, 
        Guid? jobId = null,
        TimeSpan? timeout = null)
    {
        var currentTimeout = timeout ?? _defaultTimeout;
        var command = GetDoJobCommand(input, true, maxNrOfRetries, minBackoff, maxBackoff, jobId);
        return await _masterActor.Ask<JobCreatedCommandResult>(command, currentTimeout);
    }

    public async Task<JobDoneCommandResult> DoJobAsync(TIn input, 
        int? maxNrOfRetries = null, 
        TimeSpan? minBackoff = null,
        TimeSpan? maxBackoff = null, 
        Guid? jobId = null,
        TimeSpan? timeout = null)
    {
        var currentTimeout = timeout ?? _defaultTimeout;
        var command = GetDoJobCommand(input, false, maxNrOfRetries, minBackoff, maxBackoff, jobId);
        return await _masterActor.Ask<JobDoneCommandResult>(command, currentTimeout);
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
    
    private DoJobCommand<TIn> GetDoJobCommand(TIn input, bool isCreatedCommand,
        int? maxNrOfRetries = null, TimeSpan? minBackoff = null, TimeSpan? maxBackoff = null,   Guid? jobId = null)
    {
        var id = jobId ?? Guid.NewGuid();
        return new DoJobCommand<TIn>(input,
            id,
            GetGroupName(),
            minBackoff ?? _defaultMinBackoff,
            maxBackoff ?? _defaultMaxBackoff,
            maxNrOfRetries ?? _defaultMaxNrOfRetries,
            isCreatedCommand);
    }
}