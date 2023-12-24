using Job.Api.JobExamples.SimpleLoop;
using Akka.Jobs.Interfaces;
using Akka.Jobs.Models;
using Akka.Jobs.Theater.ActorQueries.Messages.States;
using Microsoft.AspNetCore.Mvc;

namespace Job.Api.Controllers;

[ApiController]
[Route("[controller]/[action]")]
public class ForEachJobController : ControllerBase
{
    private readonly IJobContext<ForEachJobInput, ForEachJobResult> _jobContext;
    
    public ForEachJobController(IJobContext<ForEachJobInput, ForEachJobResult> jobContext)
    {
        _jobContext = jobContext;
    }
    
    [HttpPost]
    public async Task<JobCreatedCommandResult> CreateJob([FromBody] ForEachJobInput input)
    {
        return await _jobContext.CreateJobAsync(input);
    }
    
    [HttpPost]
    public async Task<JobDoneCommandResult> DoJob([FromBody] ForEachJobInput input)
    {
        return await _jobContext.DoJobAsync(input);
    }
    
    [HttpPost]
    public async Task BatchJobs([FromQuery] int input)
    {
        var list = Enumerable
            .Range(0, input)
            .Select(x => _jobContext.CreateJobAsync(new ForEachJobInput { Count = x % 2 == 0 ? 4 : 2 }));
        await Task.WhenAll(list);
    }
    
    [HttpPost]
    public async Task<StopJobCommandResult> StopJob([FromBody] Guid jobId)
    {
        return await _jobContext.StopJobAsync(jobId.ToString());
    }
    
    [HttpGet]
    public async Task<ICollection<ReplyWorkerInfo<ForEachJobResult>>> GetAllJobs([FromQuery] int requestId)
    {
        var result = await _jobContext
            .GetAllJobsCurrentStatesAsync(requestId, TimeSpan.FromMilliseconds(5000));
        return result.Values.ToList();
    }
}