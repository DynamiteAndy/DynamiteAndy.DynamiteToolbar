using DynamiteAndy.DynamiteToolbar.Services;

namespace DynamiteAndy.DynamiteToolbar.Tests.Unit.Services;

[TestFixture]
public class PubSubServiceTests
{
    private PubSubService _sut = null!;

    [SetUp]
    public void SetUp() => _sut = new PubSubService();

    [TearDown]
    public void TearDown() => _sut.Dispose();

    // -----------------------------------------------------------------------
    // Subscribe / Raise
    // -----------------------------------------------------------------------

    [Test]
    public void Raise_WithNoSubscribers_DoesNotThrow()
    {
        //Arrange + Act + Assert
        Assert.DoesNotThrow(() => _sut.Raise("orphan"));
    }

    [Test]
    public void Raise_WithOneSubscriber_InvokesHandler()
    {
        //Arrange
        string? received = null;
        _sut.Subscribe<string>(s => received = s);

        //Act
        _sut.Raise("hello");

        //Assert
        Assert.That(received, Is.EqualTo("hello"));
    }

    [Test]
    public void Raise_WithMultipleSubscribers_InvokesAllHandlers()
    {
        //Arrange
        var results = new List<int>();
        _sut.Subscribe<int>(i => results.Add(i * 1));
        _sut.Subscribe<int>(i => results.Add(i * 2));

        //Act
        _sut.Raise(3);

        //Assert
        Assert.That(results, Is.EquivalentTo([3, 6]));
    }

    [Test]
    public void Raise_DifferentTypes_OnlyInvokesMatchingHandler()
    {
        //Arrange
        var intReceived = 0;
        string? strReceived = null;

        _sut.Subscribe<int>(i => intReceived = i);
        _sut.Subscribe<string>(s => strReceived = s);

        //Act
        _sut.Raise(42);

        //Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(intReceived, Is.EqualTo(42));
            Assert.That(strReceived, Is.Null);
        }
    }

    [Test]
    public void Raise_MultipleRaiseCalls_EachDeliveredInOrder()
    {
        //Arrange
        var received = new List<string>();
        _sut.Subscribe<string>(s => received.Add(s));

        //Act
        _sut.Raise("first");
        _sut.Raise("second");

        //Assert
        Assert.That(received, Is.EqualTo(["first", "second"]));
    }

    [Test]
    public void Raise_SameHandlerSubscribedTwice_InvokedTwice()
    {
        //Arrange
        var count = 0;
        void handler(int _) => count++;

        _sut.Subscribe((Action<int>)handler);
        _sut.Subscribe((Action<int>)handler);

        //Act
        _sut.Raise(0);

        //Assert
        Assert.That(count, Is.EqualTo(2));
    }

    // -----------------------------------------------------------------------
    // Unsubscribe
    // -----------------------------------------------------------------------

    [Test]
    public void Unsubscribe_PreviouslySubscribedHandler_IsNoLongerInvoked()
    {
        //Arrange
        var count = 0;
        void handler(int _) => count++;

        _sut.Subscribe<int>(handler);
        _sut.Unsubscribe<int>(handler);

        //Act
        _sut.Raise(1);

        //Assert
        Assert.That(count, Is.Zero);
    }

    [Test]
    public void Unsubscribe_OneOfTwoHandlers_OtherHandlerStillInvoked()
    {
        //Arrange
        var aCount = 0;
        var bCount = 0;

        void aHandler(int _) => aCount++;
        void bHandler(int _) => bCount++;

        _sut.Subscribe((Action<int>)aHandler);
        _sut.Subscribe((Action<int>)bHandler);
        _sut.Unsubscribe((Action<int>)aHandler);

        //Act
        _sut.Raise(1);

        //Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(aCount, Is.Zero);
            Assert.That(bCount, Is.EqualTo(1));
        }
    }

    [Test]
    public void Unsubscribe_HandlerNotSubscribed_DoesNotThrow()
    {
        //Arrange + Act + Assert
        Assert.DoesNotThrow(() => _sut.Unsubscribe<string>(_ => { }));
    }

    [Test]
    public void Unsubscribe_LastHandlerForType_RaiseBecomesNoOp()
    {
        //Arrange
        var count = 0;
        void handler(int _) => count++;

        _sut.Subscribe<int>(handler);
        _sut.Unsubscribe<int>(handler);

        //Act + Assert
        Assert.DoesNotThrow(() => _sut.Raise(99));
        Assert.That(count, Is.Zero);
    }

    [Test]
    public void Unsubscribe_SameHandlerSubscribedTwice_RemovesOneInstance()
    {
        //Arrange
        var count = 0;
        void handler(int _) => count++;

        _sut.Subscribe<int>(handler);
        _sut.Subscribe<int>(handler);
        _sut.Unsubscribe<int>(handler);

        //Act
        _sut.Raise(1);

        //Assert
        Assert.That(count, Is.EqualTo(1));
    }

    // -----------------------------------------------------------------------
    // Dispose
    // -----------------------------------------------------------------------

    [Test]
    public void Dispose_ClearsAllHandlers_RaiseBecomesNoOp()
    {
        //Arrange
        var count = 0;
        _sut.Subscribe<int>(_ => count++);

        //Act
        _sut.Dispose();
        _sut.Raise(1);

        //Assert
        Assert.That(count, Is.Zero);
    }

    [Test]
    public void Dispose_CalledMultipleTimes_DoesNotThrow()
    {
        //Arrange + Act + Assert
        Assert.DoesNotThrow(() =>
        {
            _sut.Dispose();
            _sut.Dispose();
        });
    }
}
