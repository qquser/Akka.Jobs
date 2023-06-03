using Akka.Actor;
using Job.Core.Interfaces;
using Job.Core.Models;
using Job.Core.Theater.Workers.Messages;

namespace Job.Core.Services;

internal class JobContext<TData> : IJobContext<TData> where TData : IJobData
{
    private readonly IActorRef _jobContext;
    
    public JobContext(IActorRef jobContext)
    {
        _jobContext = jobContext;
    }

    public Guid CreateJob()
    {
        var jobId = Guid.NewGuid();
        _jobContext.Tell(new DoJobCommand(jobId, typeof(TData)));
        return jobId;
    }

    public async Task<JobCommandResult> DoJobAsync()
    {
        var jobId = Guid.NewGuid();
        return await _jobContext.Ask<JobCommandResult>(
            new DoJobCommand(jobId, typeof(TData)));
    }

    public async Task<StopJobCommandResult> StopJobAsync(Guid jobId)
    {
        return await _jobContext.Ask<StopJobCommandResult>(
            new StopJobCommand(jobId, typeof(TData)));
    }
}