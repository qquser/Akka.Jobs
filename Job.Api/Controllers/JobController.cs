using Akka.Actor;
using Job.Core.Interfaces;
using Job.Core.Models;
using Job.Core.Models.Enums;
using Job.Core.Theater.Workers.Messages;
using Microsoft.AspNetCore.Mvc;

namespace Job.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class JobController : ControllerBase
{
    private readonly IActorRef _jobContext;
    
    public JobController(IActorRef jobContext)
    {
        _jobContext = jobContext;
    }
    
    [HttpPost]
    [Route(nameof(CreateJob))]
    public async Task<Guid> CreateJob()
    {
        var id = Guid.NewGuid();
        var result = await _jobContext.Ask<StartJobCommandResult>(new StartJobCommand
        {
            JobId = id,
            GroupId = "test"
        });
        return id;
    }
    
    [HttpPost]
    [Route(nameof(StopJob))]
    public StopJobCommandResult StopJob([FromBody] Guid jobId)
    {
        throw new NotImplementedException();
    }
    
    [HttpGet]
    [Route(nameof(GetJob))]
    public (TestJob, JobState) GetJob([FromQuery] Guid jobId)
    {
        throw new NotImplementedException();
    }
}

public class ForEachJob : IJob//<TestJob>
{
    private bool _stopped;
    public bool StartJob()
    {
        foreach (var item in Enumerable.Range(0, 10))
        {
            if (_stopped)
                return false;
            Console.WriteLine($"Job {item}");
            Thread.Sleep(1000);
        }

        return true;
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