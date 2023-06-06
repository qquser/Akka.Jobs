﻿using Job.Core.Interfaces;
using Job.Core.Models;
using Microsoft.AspNetCore.Mvc;

namespace Job.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class ForEachJobController : ControllerBase
{
    private readonly IJobContext<TestJobInput, TestJobResult> _jobContext;
    
    public ForEachJobController(IJobContext<TestJobInput, TestJobResult> jobContext)
    {
        _jobContext = jobContext;
    }
    
    [HttpPost]
    [Route(nameof(CreateJob))]
    public Guid CreateJob([FromBody] TestJobInput input)
    {
        return _jobContext.CreateJob(input);
    }
    
    [HttpPost]
    [Route(nameof(DoJob))]
    public async Task<JobCommandResult> DoJob([FromBody] TestJobInput input)
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
    public async Task<ICollection<TestJobResult?>> GetAllJobs()
    {
        var result = await _jobContext
            .GetAllJobsCurrentStatesAsync(0);
        return result.Values.Select(x=>x.Result).ToList();
    }
}

public class ForEachJob : IJob<TestJobInput, TestJobResult>
{
    private int _currentState;

    private readonly ILogger<ForEachJob> _logger;
    public ForEachJob(ILogger<ForEachJob> logger)
    {
        _logger = logger;
    }
    
    public async Task<bool> DoAsync(TestJobInput input, CancellationToken token)
    {
        foreach (var item in Enumerable.Range(0, input.Count))
        {
            if (token.IsCancellationRequested)
                return false;
            _currentState = item;
            _logger.LogInformation(item.ToString());
            await Task.Delay(1000, token);
        }

        return true;
    }

    public TestJobResult GetCurrentState(Guid jobId)
    {
        return new TestJobResult
        {
            Id = jobId,
            Data = _currentState
        };
    }
}

public class TestJobInput : IJobInput
{
    public int Count { get; set; }
}

public class TestJobResult : IJobResult
{
    public Guid Id { get; set; }
    public int Data { get; set; }
}