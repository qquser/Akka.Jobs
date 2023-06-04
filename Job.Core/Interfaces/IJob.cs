namespace Job.Core.Interfaces;

public interface IJob<in TIn, out TOut> 
    where TOut : IJobResult 
    where TIn : IJobInput
{
    Task<bool> DoAsync(TIn input, CancellationToken token);
    TOut GetCurrentState(Guid jobId);
}