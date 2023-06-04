using Akka.Actor;
using Job.Core.Interfaces;
using Job.Core.Models;
using Job.Core.Theater.Workers.Messages;

namespace Job.Core.Services;

internal class JobContext<TIn, TOut>  : IJobContext<TIn, TOut> 
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

    public Guid CreateJob(TIn input, Guid? jobId = null)
    {
        var id = jobId ?? Guid.NewGuid();
        _jobContext.Tell(new DoJobCommand(input, 
            typeof(TIn), 
            typeof(TOut), 
            id,
            GetGroupName()));
        return id;
    }

    public async Task<JobCommandResult> DoJobAsync(TIn input, Guid? jobId = null)
    {
        var id = jobId ?? Guid.NewGuid();
        return await _jobContext.Ask<JobCommandResult>(
            new DoJobCommand(input,
                typeof(TIn),
                typeof(TOut),
                id,
                GetGroupName()));
    }

    public async Task<StopJobCommandResult> StopJobAsync(Guid jobId)
    {
        return await _jobContext.Ask<StopJobCommandResult>(
            new StopJobCommand(jobId, GetGroupName()));
    }
}