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

public class ActorServiceProviderPropsWithScopesSpecs 
    : TestKit, IClassFixture<AkkaDiFixture<TestForEachJobInput, TestForEachJobResult, TestForEachJob>>
{
    private readonly AkkaDiFixture<TestForEachJobInput, TestForEachJobResult, TestForEachJob> _fixture;
    private readonly ActorSystem? _actorSystem;
    public ActorServiceProviderPropsWithScopesSpecs(
        AkkaDiFixture<TestForEachJobInput, TestForEachJobResult, TestForEachJob> fixture)
    {
        _fixture = fixture;
        _actorSystem = _fixture.Provider.GetService<ActorSystem>();
    }
    
    private DoJobCommand<TestForEachJobInput> GetDoJobCommand(TestForEachJobInput input, Guid? jobId = null)
    {
        var id = jobId ?? Guid.NewGuid();
        return new DoJobCommand<TestForEachJobInput>(input,
            id,
            "Test",
            TimeSpan.FromSeconds(1),
            TimeSpan.FromSeconds(3),
            0,
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
}
