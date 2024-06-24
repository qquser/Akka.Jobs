using Job.Api.JobExamples.SimpleLoop;
using Akka.Jobs.Interfaces;
using Akka.Jobs.Models;
using Akka.Jobs.Theater.ActorQueries.Messages.States;
using Microsoft.AspNetCore.Mvc;

namespace Job.Api.Controllers;

[ApiController]
[Route("[controller]/[action]")]
public class ForEachJobController(IJobContext<ForEachJobInput, ForEachJobResult> jobContext) : ControllerBase
{
    [HttpPost]
    public Task<JobCreatedCommandResult> CreateJob([FromBody] ForEachJobInput input)
    {
        return jobContext.CreateJobAsync(input);
    }
    
    [HttpPost]
    public Task<JobDoneCommandResult> DoJob([FromBody] ForEachJobInput input)
    {
        return jobContext.DoJobAsync(input);
    }
    
    [HttpPost]
    public async Task BatchJobs([FromQuery] int input)
    {
        var list = Enumerable
            .Range(0, input)
            .Select(x => jobContext.CreateJobAsync(new ForEachJobInput { Count = x % 2 == 0 ? 4 : 2 }));
        await Task.WhenAll(list);
    }
    
    [HttpPost]
    public Task<StopJobCommandResult> StopJob([FromBody] Guid jobId)
    {
        return jobContext.StopJobAsync(jobId.ToString());
    }
    
    [HttpGet]
    public async Task<ICollection<ReplyWorkerInfo<ForEachJobResult>>> GetAllJobs([FromQuery] int requestId)
    {
        var result = await jobContext
            .GetAllJobsCurrentStatesAsync(requestId, TimeSpan.FromMilliseconds(5000));
        return result.Values.ToList();
    }
}