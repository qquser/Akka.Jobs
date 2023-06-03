using Job.Core.Models.Enums;

namespace Job.Core.Interfaces;

public interface IJob//<TData> where TData : IJobData
{
    bool StartJob();
    bool StopJob();

}

public interface IJobData
{
    public Guid Id { get; set; }
}