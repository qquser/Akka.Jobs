using Job.Api.JobExamples.SimpleLoop;
using Akka.Jobs.Interfaces;
using Akka.Jobs.Models;
using Microsoft.AspNetCore.Mvc;

namespace Job.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class ForEachJobController : ControllerBase
{
    private readonly IJobContext<ForEachJobInput, ForEachJobResult> _jobContext;
    
    public ForEachJobController(IJobContext<ForEachJobInput, ForEachJobResult> jobContext)
    {
        _jobContext = jobContext;
    }
    
    [HttpPost]
    [Route(nameof(CreateJob))]
    public async Task<JobCreatedCommandResult> CreateJob([FromBody] ForEachJobInput input)
    {
        return await _jobContext.CreateJobAsync(input);
    }
    
    [HttpPost]
    [Route(nameof(DoJob))]
    public async Task<JobDoneCommandResult> DoJob([FromBody] ForEachJobInput input)
    {
        return await _jobContext.DoJobAsync(input);
    }
    
    [HttpPost]
    [Route(nameof(BatchJobs))]
    public async Task BatchJobs([FromQuery] int input)
    {
        var list = Enumerable
            .Range(0, input)
            .Select(_ => _jobContext.CreateJobAsync(new ForEachJobInput{Count = 100})); 
        await Task.WhenAll(list);
    }
    
    [HttpPost]
    [Route(nameof(StopJob))]
    public async Task<StopJobCommandResult> StopJob([FromBody] Guid jobId)
    {
        return await _jobContext.StopJobAsync(jobId.ToString());
    }
    
    [HttpGet]
    [Route(nameof(GetAllJobs))]
    public async Task<ICollection<ForEachJobResult?>> GetAllJobs([FromQuery] int requestId)
    {
        var result = await _jobContext
            .GetAllJobsCurrentStatesAsync(requestId);
        return result.Values.Select(x => x.Result).ToList();
    }
}