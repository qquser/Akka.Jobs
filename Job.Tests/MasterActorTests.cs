using Akka.Actor;
using Akka.DependencyInjection;
using Akka.TestKit.Xunit2;
using Akka.Jobs.Models;
using Akka.Jobs.Theater.Master;
using Akka.Jobs.Theater.Master.Groups.Workers.Messages;
using Job.Tests.JobTests;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Job.Tests;

public class MasterActorTests 
    : TestKit,
        IClassFixture<AkkaDiFixture<TestForEachJobInput, TestForEachJobResult, TestForEachJob>>,
        IClassFixture<AkkaDiFixtureWithoutJob<TestForEachJobInput, TestForEachJobResult, TestForEachJob>>,
        IClassFixture<AkkaDiFixture<TestExceptionForEachJobInput, TestForEachJobResult, TestExceptionForEachJob>>
{
    private readonly ActorSystem? _actorSystem;
    private readonly ActorSystem? _actorSystemWithoutJob;
    private readonly ActorSystem? _actorSystemExceptionJob;
    private readonly string _testGroupName = "Test";
    public MasterActorTests(
        AkkaDiFixture<TestForEachJobInput, TestForEachJobResult, TestForEachJob> fixture,
        AkkaDiFixtureWithoutJob<TestForEachJobInput, TestForEachJobResult, TestForEachJob> fixtureWithoutJob,
        AkkaDiFixture<TestExceptionForEachJobInput, TestForEachJobResult, TestExceptionForEachJob> fixtureExceptionForEachJob)
    {
        _actorSystem = fixture.Provider!.GetService<ActorSystem>();
        _actorSystemWithoutJob = fixtureWithoutJob.Provider!.GetService<ActorSystem>();
        _actorSystemExceptionJob = fixtureExceptionForEachJob.Provider!.GetService<ActorSystem>();
    }
    
    private DoJobCommand<TestForEachJobInput> GetDoJobCommand(TestForEachJobInput input, string? jobId = null)
    {
        var id = jobId ?? Guid.NewGuid().ToString();
        return new DoJobCommand<TestForEachJobInput>(input,
            id,
            _testGroupName,
            TimeSpan.FromMicroseconds(100),
            TimeSpan.FromMicroseconds(300),
            3,
            false);
    }
    
    private DoJobCommand<TestExceptionForEachJobInput> GetDoJobExceptionCommand(TestExceptionForEachJobInput input, string? jobId = null)
    {
        var id = jobId ?? Guid.NewGuid().ToString();
        return new DoJobCommand<TestExceptionForEachJobInput>(input,
            id,
            _testGroupName,
            TimeSpan.FromMicroseconds(100),
            TimeSpan.FromMicroseconds(300),
            3,
            false);
    }
    
    [Fact]
    public void MasterActor_ShouldTell_WhenSimpleTest()
    {
        var masterActorProps = DependencyResolver
            .For(_actorSystem)
            .Props<MasterActor<TestForEachJobInput, TestForEachJobResult>>();
        var masterActor = _actorSystem!.ActorOf(masterActorProps);
        var input = new TestForEachJobInput { Count = 1};
        var probe = CreateTestProbe();
        
        masterActor.Tell(GetDoJobCommand(input), probe.Ref);
        var result = probe.ExpectMsg<JobDoneCommandResult>(TimeSpan.FromSeconds(30));
        
        Assert.True(result.Success);
    }
    
    [Fact]
    public void MasterActor_ShouldAnswerFalse_WhenAllAttemptsFailed()
    {
        var masterActorProps = DependencyResolver
            .For(_actorSystemExceptionJob)
            .Props<MasterActor<TestExceptionForEachJobInput, TestForEachJobResult>>();
        var masterActor = _actorSystemExceptionJob!.ActorOf(masterActorProps);
        var input = new TestExceptionForEachJobInput { Count = 1};
        var probe = CreateTestProbe();
        
        masterActor.Tell(GetDoJobExceptionCommand(input), probe.Ref);
        var result = probe.ExpectMsg<JobDoneCommandResult>(TimeSpan.FromSeconds(30));
        
        Assert.False(result.Success);
    }
    
    [Fact]
    public void MasterActor_ShouldAnswerFalse_WhenJobIdEmptyString()
    {
        var masterActorProps = DependencyResolver
            .For(_actorSystem)
            .Props<MasterActor<TestForEachJobInput, TestForEachJobResult>>();
        var masterActor = _actorSystem!.ActorOf(masterActorProps);
        var input = new TestForEachJobInput { Count = 10};
        var probe = CreateTestProbe();
        var doJobCommand = GetDoJobCommand(input, string.Empty);

        masterActor.Tell(doJobCommand, probe.Ref);
        var result = probe.ExpectMsg<JobDoneCommandResult>(TimeSpan.FromSeconds(30));
        
        Assert.False(result.Success);
    }
    
    [Fact]
    public async Task MasterActor_ShouldStop_WhenSimpleTest()
    {
        var masterActorProps = DependencyResolver
            .For(_actorSystem)
            .Props<MasterActor<TestForEachJobInput, TestForEachJobResult>>();
        var masterActor = _actorSystem!.ActorOf(masterActorProps);
        var input = new TestForEachJobInput { Count = 1000};
        var probe = CreateTestProbe();
        var doJobCommand = GetDoJobCommand(input);
        var stopJobCommand = new StopJobCommand(doJobCommand.JobId, _testGroupName);
        
        masterActor.Tell(doJobCommand, probe.Ref);
        await Task.Delay(1000);
        masterActor.Tell(stopJobCommand);
        var result = probe.ExpectMsg<JobDoneCommandResult>(TimeSpan.FromSeconds(30));
        
        Assert.False(result.Success);
    }
    
    [Fact]
    public void MasterActor_ShouldAnswerFalse_WhenIJobNotRegistered()
    {
        var masterActorProps = DependencyResolver
            .For(_actorSystemWithoutJob)
            .Props<MasterActor<TestForEachJobInput, TestForEachJobResult>>();
        var masterActor = _actorSystemWithoutJob!.ActorOf(masterActorProps);
        var input = new TestForEachJobInput { Count = 1};
        var probe = CreateTestProbe();
        
        masterActor.Tell(GetDoJobCommand(input), probe.Ref);
        var result = probe.ExpectMsg<JobDoneCommandResult>(TimeSpan.FromSeconds(30));
        
        Assert.False(result.Success);
    }
}
