using Job.Core.Interfaces;
using Job.Core.Models;
using Job.Core.Models.Enums;
using Microsoft.AspNetCore.Mvc;

namespace Job.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class JobController : ControllerBase
{
    private readonly IJobContext<TestJob> _jobContext;
    
    public JobController(IJobContext<TestJob> jobContext)
    {
        _jobContext = jobContext;
    }
    
    [HttpPost]
    [Route(nameof(CreateJob))]
    public Guid CreateJob()
    {
        return _jobContext.CreateJob();
    }
    
    [HttpPost]
    [Route(nameof(DoJob))]
    public async Task<JobCommandResult> DoJob()
    {
        return await _jobContext.DoJobAsync();
    }
    
    [HttpPost]
    [Route(nameof(StopJob))]
    public async Task<StopJobCommandResult> StopJob([FromBody] Guid jobId)
    {
        return await _jobContext.StopJobAsync(jobId);
    }
    
    [HttpGet]
    [Route(nameof(GetJob))]
    public (TestJob, JobState) GetJob([FromQuery] Guid jobId)
    {
        throw new NotImplementedException();
    }
}

public class ForEachJob : IJob<TestJob>
{
    private bool _stopped;
    private int _currentState;
    public bool DoJob()
    {
        foreach (var item in Enumerable.Range(0, 10))
        {
            if (_stopped)
                return false;
            _currentState = item;
            Console.WriteLine($"Job {item}");
            Thread.Sleep(1000);
        }

        return true;
    }

    public bool StopJob(Guid jobId)
    {
        throw new NotImplementedException();
    }

    public TestJob GetCurrentState(Guid jobId)
    {
        return new TestJob
        {
            Id = jobId,
            Data = _currentState
        };
    }

    public bool StopJob()
    {
        _stopped = true;
        return true;
    }

}

public class TestJob : IJobData
{
    public Guid Id { get; set; }
    public int Data { get; set; }
}