namespace Job.Core.Interfaces;

public interface IJob<in TIn, TOut>
    where TIn : IJobInput
    where TOut : IJobResult 
{
    Task<bool> DoAsync(TIn input, CancellationToken token);
    TOut GetCurrentState(Guid jobId);
}