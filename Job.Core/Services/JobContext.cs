using Akka.Actor;
using Job.Core.Interfaces;
using Job.Core.Models;
using Job.Core.Theater.Master.Groups.Workers.Messages;

namespace Job.Core.Services;

internal class JobContext<TIn, TOut> : IJobContext<TIn, TOut> 
    where TIn : IJobInput
    where TOut : IJobResult
{
    private readonly IActorRef _jobContext;
    
    public JobContext(IActorRef jobContext)
    {
        _jobContext = jobContext;
    }

    private string GetGroupName()
    {
        var type = GetType().ToString();
        return type;
    }

    public Guid CreateJob(TIn input,
        int? maxNrOfRetries = null, TimeSpan? minBackoff = null, TimeSpan? maxBackoff = null,  Guid? jobId = null)
    {
        var id = jobId ?? Guid.NewGuid();
        _jobContext.Tell(new DoJobCommand(input, 
            typeof(TIn), 
            typeof(TOut), 
            id,
            GetGroupName(),
            minBackoff ?? TimeSpan.FromSeconds(1),
            maxBackoff ?? TimeSpan.FromSeconds(3),
            maxNrOfRetries ?? 5));
        return id;
    }

    public async Task<JobCommandResult> DoJobAsync(TIn input, 
        int? maxNrOfRetries = null, TimeSpan? minBackoff = null, TimeSpan? maxBackoff = null,  Guid? jobId = null)
    {
        var id = jobId ?? Guid.NewGuid();
        return await _jobContext.Ask<JobCommandResult>(
            new DoJobCommand(input,
                typeof(TIn),
                typeof(TOut),
                id,
                GetGroupName(),
                minBackoff ?? TimeSpan.FromSeconds(1),
                maxBackoff ?? TimeSpan.FromSeconds(3),
                maxNrOfRetries ?? 5));
    }

    public async Task<StopJobCommandResult> StopJobAsync(Guid jobId)
    {
        return await _jobContext.Ask<StopJobCommandResult>(
            new StopJobCommand(jobId, GetGroupName()));
    }

    public Task<TOut> GetCurrentStateAsync(Guid jobId)
    {
        throw new NotImplementedException();
    }
}