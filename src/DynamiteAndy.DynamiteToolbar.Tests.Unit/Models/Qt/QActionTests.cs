using DynamiteAndy.DynamiteToolbar.Interop.Bridge.Qt;
using DynamiteAndy.DynamiteToolbar.Models.Qt;
using Moq;

namespace DynamiteAndy.DynamiteToolbar.Tests.Unit.Models.Qt;

[TestFixture]
public class QActionTests
{
    private Mock<IQActionShim> _shimMock = null!;
    private static readonly IntPtr Handle = new(0x300);

    [SetUp]
    public void SetUp() => _shimMock = new Mock<IQActionShim>();

    private QAction Create() => new(Handle, _shimMock.Object);

    // -----------------------------------------------------------------------
    // Text
    // -----------------------------------------------------------------------

    [Test]
    public void SetText_CallsShim()
    {
        //Arrange
        var action = Create();

        //Act
        action.Text = "Settings";

        //Assert
        _shimMock.Verify(s => s.SetText(Handle, "Settings"), Times.Once);
    }

    [Test]
    public void SetText_SameValueTwice_CallsShimOnlyOnce()
    {
        //Arrange
        var action = Create();

        //Act
        action.Text = "same";
        action.Text = "same";

        //Assert
        _shimMock.Verify(s => s.SetText(Handle, "same"), Times.Once);
    }

    [Test]
    public void SetText_Null_DoesNotCallShim()
    {
        //Arrange
        var action = Create();

        //Act
        action.Text = null;

        //Assert
        _shimMock.Verify(s => s.SetText(It.IsAny<IntPtr>(), It.IsAny<string>()), Times.Never);
    }

    [Test]
    public void SetText_ChangedValue_CallsShimAgain()
    {
        //Arrange
        var action = Create();

        //Act
        action.Text = "A";
        action.Text = "B";

        //Assert
        _shimMock.Verify(s => s.SetText(Handle, "A"), Times.Once);
        _shimMock.Verify(s => s.SetText(Handle, "B"), Times.Once);
    }

    // -----------------------------------------------------------------------
    // RegisterCallback
    // -----------------------------------------------------------------------

    [Test]
    public void RegisterCallback_CallsOnTriggeredWithNonZeroPtr()
    {
        //Arrange
        var action = Create();

        //Act
        action.RegisterCallback();

        //Assert
        _shimMock.Verify(s => s.OnTriggered(Handle, It.Is<IntPtr>(p => p != IntPtr.Zero)), Times.Once);
    }

    // -----------------------------------------------------------------------
    // Dispose
    // -----------------------------------------------------------------------

    [Test]
    public void Dispose_ClearsCallback()
    {
        //Arrange
        var action = Create();

        //Act
        action.RegisterCallback();
        action.Dispose();

        //Assert
        _shimMock.Verify(s => s.OnTriggered(Handle, IntPtr.Zero), Times.Once);
    }

    [Test]
    public void Dispose_CalledTwice_ClearsCallbackOnlyOnce()
    {
        //Arrange
        var action = Create();

        //Act
        action.RegisterCallback();
        action.Dispose();
        action.Dispose();

        //Assert
        _shimMock.Verify(s => s.OnTriggered(Handle, IntPtr.Zero), Times.Once);
    }
}
