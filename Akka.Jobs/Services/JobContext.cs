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
    /// <summary>
    /// MasterActor is registered here
    /// </summary>
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

    
    public Task<JobCreatedCommandResult> CreateJobAsync(TIn input,
        int? maxNrOfRetries = null,
        TimeSpan? minBackoff = null,
        TimeSpan? maxBackoff = null, 
        string? jobId = null,
        TimeSpan? timeout = null)
    {
        var currentTimeout = timeout ?? _defaultTimeout;
        var command = GetDoJobCommand(input, true, maxNrOfRetries, minBackoff, maxBackoff, jobId);
        //Waits for JobCreatedCommandResult response from MasterActor.DoJobCommandHandler
        return _masterActor.Ask<JobCreatedCommandResult>(command, currentTimeout); 
    }

    public Task<JobDoneCommandResult> DoJobAsync(TIn input, 
        int? maxNrOfRetries = null, 
        TimeSpan? minBackoff = null,
        TimeSpan? maxBackoff = null, 
        string? jobId = null,
        TimeSpan? timeout = null)
    {
        var currentTimeout = timeout ?? _defaultTimeout;
        var command = GetDoJobCommand(input, false, maxNrOfRetries, minBackoff, maxBackoff, jobId);
        //Waits for JobDoneCommandResult response from MasterActor.DoJobCommandHandler
        return _masterActor.Ask<JobDoneCommandResult>(command, currentTimeout); 
    }

    public Task<StopJobCommandResult> StopJobAsync(string jobId, TimeSpan? timeout = null)
    {
        var currentTimeout = timeout ?? _defaultTimeout;
        //Waits StopJobCommandResult response from MasterActor.StopJobCommandHandler
        return _masterActor.Ask<StopJobCommandResult>( 
            new StopJobCommand(jobId, GetGroupName()), currentTimeout);
    }

    public async Task<IDictionary<string, ReplyWorkerInfo<TOut>>> GetAllJobsCurrentStatesAsync(long requestId,
        TimeSpan? timeout = null)
    {
        var currentTimeout = timeout ?? _defaultTimeout;
        var query = new RequestAllWorkersInfo(requestId, GetGroupName(), currentTimeout);
        //Waits RespondAllWorkersInfo response from MasterActor.RequestAllWorkersInfoQueryHandler
        RespondAllWorkersInfo<TOut> info = await _masterActor
            .Ask<RespondAllWorkersInfo<TOut>>(query, currentTimeout.Add(TimeSpan.FromSeconds(2)));
        return info.WorkersData;
    }
    
    private string GetGroupName()
    {
        return Regex.Replace(GetType().ToString(), @"[^\w\d]", "_");;
    }
    
    private DoJobCommand<TIn> GetDoJobCommand(TIn input, bool isCreatedCommand,
        int? maxNrOfRetries = null, TimeSpan? minBackoff = null, TimeSpan? maxBackoff = null,  string? jobId = null)
    {
        var id = jobId ?? Guid.NewGuid().ToString();
        return new DoJobCommand<TIn>(input,
            id,
            GetGroupName(),
            minBackoff ?? _defaultMinBackoff,
            maxBackoff ?? _defaultMaxBackoff,
            maxNrOfRetries ?? _defaultMaxNrOfRetries,
            isCreatedCommand);
    }
}