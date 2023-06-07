using Job.Api.JobExamples.SimpleLoop;
using Job.Core.Interfaces;
using Job.Core.Models;
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
    public Guid CreateJob([FromBody] ForEachJobInput input)
    {
        return _jobContext.CreateJob(input);
    }
    
    [HttpPost]
    [Route(nameof(DoJob))]
    public async Task<JobCommandResult> DoJob([FromBody] ForEachJobInput input)
    {
        return await _jobContext.DoJobAsync(input);
    }
    
    [HttpPost]
    [Route(nameof(StopJob))]
    public async Task<StopJobCommandResult> StopJob([FromBody] Guid jobId)
    {
        return await _jobContext.StopJobAsync(jobId);
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