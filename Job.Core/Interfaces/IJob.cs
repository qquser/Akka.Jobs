using Job.Core.Models.Enums;

namespace Job.Core.Interfaces;

public interface IJob<out TData> where TData : IJobData
{
    bool DoJob();
    bool StopJob(Guid jobId);
    TData GetCurrentState(Guid jobId);
}

public interface IJobData
{
    public Guid Id { get; set; }
}