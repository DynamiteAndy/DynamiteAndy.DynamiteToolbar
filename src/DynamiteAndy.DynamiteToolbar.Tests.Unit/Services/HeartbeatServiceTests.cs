using DynamiteAndy.DynamiteToolbar.Models.Events;
using DynamiteAndy.DynamiteToolbar.Services;
using Microsoft.Extensions.Time.Testing;
using Moq;

namespace DynamiteAndy.DynamiteToolbar.Tests.Unit.Services;

[TestFixture]
public class HeartbeatServiceTests
{
    private Mock<IPubSubService> _pubSubMock = null!;
    private FakeTimeProvider _timeProvider = null!;
    private HeartbeatService _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _pubSubMock = new Mock<IPubSubService>();
        _timeProvider = new FakeTimeProvider();
        _sut = new HeartbeatService(_pubSubMock.Object, _timeProvider);
    }

    [TearDown]
    public void TearDown() => _sut.Dispose();

    // -----------------------------------------------------------------------
    // OnTick
    // -----------------------------------------------------------------------

    [Test]
    public void OnTick_RaisesHeartbeatEvent()
    {
        //Arrange + Act
        _sut.OnTick();

        //Assert
        _pubSubMock.Verify(p => p.Raise(It.IsAny<HeartbeatEvent>()), Times.Once);
    }

    [Test]
    public void OnTick_CalledMultipleTimes_RaisesEachTime()
    {
        //Arrange + Act
        _sut.OnTick();
        _sut.OnTick();
        _sut.OnTick();

        //Assert
        _pubSubMock.Verify(p => p.Raise(It.IsAny<HeartbeatEvent>()), Times.Exactly(3));
    }

    // -----------------------------------------------------------------------
    // Start (timer-based integration — uses FakeTimeProvider)
    // -----------------------------------------------------------------------

    [Test]
    public void Start_AfterDueTime_RaisesAtLeastOnce()
    {
        //Arrange
        _sut.Start(TimeSpan.FromMilliseconds(50), TimeSpan.FromMilliseconds(500));

        //Act
        _timeProvider.Advance(TimeSpan.FromMilliseconds(150));

        //Assert
        _pubSubMock.Verify(p => p.Raise(It.IsAny<HeartbeatEvent>()), Times.AtLeastOnce);
    }

    [Test]
    public void Start_BeforeDueTime_HasNotFiredYet()
    {
        //Arrange
        _sut.Start(TimeSpan.FromSeconds(60), TimeSpan.FromSeconds(60));

        //Act
        _timeProvider.Advance(TimeSpan.FromSeconds(50));

        //Assert
        _pubSubMock.Verify(p => p.Raise(It.IsAny<HeartbeatEvent>()), Times.Never);
    }

    [Test]
    public void Start_WithZeroDueTime_FiresImmediately()
    {
        //Arrange + Act
        _sut.Start(TimeSpan.Zero, TimeSpan.FromSeconds(60));

        //Assert
        _pubSubMock.Verify(p => p.Raise(It.IsAny<HeartbeatEvent>()), Times.AtLeastOnce);
    }

    // -----------------------------------------------------------------------
    // Dispose
    // -----------------------------------------------------------------------

    [Test]
    public void Dispose_StopsTimer_NoMoreRaiseCallsAfterDispose()
    {
        //Arrange — let the timer fire a couple of times before disposing
        _sut.Start(TimeSpan.FromMilliseconds(50), TimeSpan.FromMilliseconds(50));
        _timeProvider.Advance(TimeSpan.FromMilliseconds(120));
        var callsAtDispose = _pubSubMock.Invocations.Count;

        //Act
        _sut.Dispose();
        _timeProvider.Advance(TimeSpan.FromMilliseconds(150));

        //Assert
        Assert.That(_pubSubMock.Invocations, Has.Count.EqualTo(callsAtDispose));
    }

    [Test]
    public void Dispose_WithoutStart_DoesNotThrow()
    {
        //Arrange + Act + Assert
        Assert.DoesNotThrow(() => _sut.Dispose());
    }

    [Test]
    public void Dispose_CalledTwice_DoesNotThrow()
    {
        //Arrange + Act + Assert
        Assert.DoesNotThrow(() =>
        {
            _sut.Dispose();
            _sut.Dispose();
        });
    }
}
