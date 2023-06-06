using Akka.Actor;
using Akka.TestKit;
using Akka.TestKit.Xunit2;
using Job.Core.Models;
using Job.Core.Theater.Master;
using Job.Core.Theater.Master.Groups.Workers.Messages;
using Job.Tests.JobTests;
using Xunit;

namespace Job.Tests;

public class MasterActorTests : TestKit
{
    private DoJobCommand<TestForEachJobInput> GetDoJobCommand(TestForEachJobInput input, Guid? jobId = null)
    {
        var id = jobId ?? Guid.NewGuid();
        return new DoJobCommand<TestForEachJobInput>(input,
            id,
            "Test",
            TimeSpan.FromSeconds(1),
            TimeSpan.FromSeconds(3),
            0);
    }
    [Fact]
    public void MasterActor_ShouldTell_WhenSimpleTest()
    {
        var probe = CreateTestProbe();

        //var masterActor2 = Sys.DI();
        var masterActor = Sys.ActorOf(Props.Create<MasterActor<TestForEachJobInput, TestForEachJobResult>>());
        var input = new TestForEachJobInput { Count = 1};
        masterActor.Tell(GetDoJobCommand(input), probe.Ref); 

        var result = probe.ExpectMsg<JobCommandResult>(TimeSpan.FromSeconds(3));
        
        Assert.True(result.Success);
    }
}