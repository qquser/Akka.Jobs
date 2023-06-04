namespace Job.Core.Interfaces;

public interface IJob<in TIn, out TOut> 
    where TOut : IJobResult 
    where TIn : IJobInput
{
    Task<bool> DoJobAsync(TIn input, CancellationToken token);
    TOut GetCurrentState(Guid jobId);
}

public interface IJobInput
{
}

public interface IJobResult
{
    public Guid Id { get; set; }
}