using Akka.Actor;
using Akka.TestKit.Xunit2;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Shouldly;
using Xunit.Abstractions;

namespace AkkaLabTest;

// How to Unit Test Akka.NET Actors with Akka.NET.TestKit
// https://petabridge.com/blog/how-to-unit-test-akkadotnet-actors-akka-testkit/

public class SimpleTestForAkkaNetActor : TestKit
{
    private readonly ITestOutputHelper _testOutputHelper;

    public SimpleTestForAkkaNetActor(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    [Fact(DisplayName = "test parent actor is able to receive child actor's message")]
    public void Test_Parent_Actor_Is_Able_To_Receive_Child_Actor_s_Message()
    {
        var mockFoo = new Mock<IFoo>();
        var services = new ServiceCollection();
        mockFoo.Setup(g => g.Bar()).Returns(It.IsAny<int>());
        services.AddTransient(o => mockFoo.Object);
        var provider = services.BuildServiceProvider();

        var probe = CreateTestProbe();
        var timeActor = Sys.ActorOf(Props.Create<TimerActor>(provider));
        timeActor.Tell("start", probe.Ref);

        probe.ExpectMsg<string>("hello");
    }

    [Fact(DisplayName = "test actor's internal state is correct")]
    public void Test_Actor_s_Internal_Stat_Is_Correct()
    {
        var mockFoo = new Mock<IFoo>();
        var services = new ServiceCollection();
        mockFoo.Setup(g => g.Bar()).Returns(It.IsAny<int>());
        services.AddTransient(o => mockFoo.Object);
        var provider = services.BuildServiceProvider();

        var targetActor = ActorOfAsTestActorRef<TimerActor>(Props.Create<TimerActor>(provider));
        targetActor.Tell("start");

        var actual = targetActor.UnderlyingActor.Message;

        actual.ShouldBe("start");
    }

    [Fact(DisplayName = "should call bar method after timer-actor received string message")]
    public void Should_Call_Bar_Method_After_TimerActor_Received_String_Message()
    {
        var mockFoo = new Mock<IFoo>();
        var services = new ServiceCollection();

        mockFoo.Setup(g => g.Bar()).Returns(It.IsAny<int>());
        services.AddTransient(o => mockFoo.Object);

        var provider = services.BuildServiceProvider();

        var probe = CreateTestProbe();
        var sutActor = Sys.ActorOf(Props.Create<TimerActor>(provider));
        sutActor.Tell(1, probe.Ref);

        // if you don't put this before verify, then the test will fail...
        // ExpectNoMsg for me it's more like the way to wait until the message received
        probe.ExpectNoMsg(250);
        mockFoo.Verify(g => g.Bar(), Times.AtLeastOnce);
    }
}

public interface IFoo
{
    int Bar();
}

public class Foo : IFoo
{
    public int Bar()
    {
        return 10328;
    }
}

public class TimerActor : ReceiveActor, IWithTimers
{
    public ITimerScheduler Timers { get; set; }

    public string Message { get; set; }

    private readonly IServiceScope _scope;

    public TimerActor(IServiceProvider sp)
    {
        _scope = sp.CreateScope();

        var foo = _scope.ServiceProvider.GetRequiredService<IFoo>();

        Receive<string>(message =>
        {
            foo.Bar();

            // change internal stat
            Message = message;

            // send message to the sender actor
            Sender.Tell("hello");
        });

        Receive<int>(_ => { foo.Bar(); });
    }

    protected override void PreStart()
    {
        Timers.StartPeriodicTimer("timer-actor-key", "start", TimeSpan.FromSeconds(0.1), TimeSpan.FromSeconds(5));
    }
}