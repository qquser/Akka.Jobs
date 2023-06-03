namespace Job.Core.Interfaces;

public interface IJob<out TData> where TData : IJobData
{
    Task<bool> DoJobAsync(CancellationToken token);
    TData GetCurrentState(Guid jobId);
}

public interface IJobData
{
    public Guid Id { get; set; }
}