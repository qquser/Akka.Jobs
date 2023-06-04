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

    public Guid CreateJob(TIn input,
        int? maxNrOfRetries = null, TimeSpan? minBackoff = null, TimeSpan? maxBackoff = null,  Guid? jobId = null)
    {
        var command = GetDoJobCommand(input, maxNrOfRetries, minBackoff, maxBackoff, jobId);
        _jobContext.Tell(command);
        return command.JobId;//
    }

    public async Task<JobCommandResult> DoJobAsync(TIn input, 
        int? maxNrOfRetries = null, TimeSpan? minBackoff = null, TimeSpan? maxBackoff = null,  Guid? jobId = null)
    {
        var command = GetDoJobCommand(input, maxNrOfRetries, minBackoff, maxBackoff, jobId);
        return await _jobContext.Ask<JobCommandResult>(command);
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
    
    private string GetGroupName()
    {
        var type = GetType().ToString();
        return type;
    }
    
    private DoJobCommand GetDoJobCommand(TIn input,
        int? maxNrOfRetries = null, TimeSpan? minBackoff = null, TimeSpan? maxBackoff = null,  Guid? jobId = null)
    {
        var id = jobId ?? Guid.NewGuid();
        return new DoJobCommand(input,
            typeof(TIn),
            typeof(TOut),
            id,
            GetGroupName(),
            minBackoff ?? TimeSpan.FromSeconds(1),
            maxBackoff ?? TimeSpan.FromSeconds(3),
            maxNrOfRetries ?? 4);
    }
}