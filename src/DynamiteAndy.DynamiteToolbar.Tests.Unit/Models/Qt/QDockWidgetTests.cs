using DynamiteAndy.DynamiteToolbar.Interop.Bridge.Qt;
using DynamiteAndy.DynamiteToolbar.Models.Qt;
using Moq;

namespace DynamiteAndy.DynamiteToolbar.Tests.Unit.Models.Qt;

[TestFixture]
public class QDockWidgetTests
{
    private Mock<IQDockWidgetShim> _shimMock = null!;
    private static readonly IntPtr Handle = new(0x400);

    [SetUp]
    public void SetUp() => _shimMock = new Mock<IQDockWidgetShim>();

    private QDockWidget Create() => new(Handle, _shimMock.Object);

    // -----------------------------------------------------------------------
    // Constructor wires top-level-changed callback
    // -----------------------------------------------------------------------

    [Test]
    public void Constructor_RegistersTopLevelChangedCallback()
    {
        //Arrange + Act
        _ = Create();

        //Assert
        _shimMock.Verify(s => s.OnTopLevelChanged(Handle, It.Is<IntPtr>(p => p != IntPtr.Zero)), Times.Once);
    }

    // -----------------------------------------------------------------------
    // Floating
    // -----------------------------------------------------------------------

    [Test]
    public void SetFloating_True_CallsShim()
    {
        //Arrange
        var dockWidget = Create();

        //Act
        dockWidget.Floating = true;

        //Assert
        _shimMock.Verify(s => s.SetFloating(Handle, true), Times.Once);
    }

    [Test]
    public void SetFloating_False_WhenAlreadyFalse_DoesNotCallShim()
    {
        //Arrange — default is false
        var dockWidget = Create();

        //Act
        dockWidget.Floating = false;

        //Assert
        _shimMock.Verify(s => s.SetFloating(It.IsAny<IntPtr>(), It.IsAny<bool>()), Times.Never);
    }

    [Test]
    public void SetFloating_SameValueTwice_CallsShimOnlyOnce()
    {
        //Arrange
        var dockWidget = Create();

        //Act
        dockWidget.Floating = true;
        dockWidget.Floating = true;

        //Assert
        _shimMock.Verify(s => s.SetFloating(Handle, true), Times.Once);
    }

    [Test]
    public void SetFloating_Toggle_CallsShimEachTime()
    {
        //Arrange
        var dockWidget = Create();

        //Act
        dockWidget.Floating = true;
        dockWidget.Floating = false;

        //Assert
        _shimMock.Verify(s => s.SetFloating(Handle, true), Times.Once);
        _shimMock.Verify(s => s.SetFloating(Handle, false), Times.Once);
    }

    // -----------------------------------------------------------------------
    // Dispose
    // -----------------------------------------------------------------------

    [Test]
    public void Dispose_ClearsTopLevelChangedCallback()
    {
        //Arrange
        var dockWidget = Create();

        //Act
        dockWidget.Dispose();

        //Assert
        _shimMock.Verify(s => s.OnTopLevelChanged(Handle, IntPtr.Zero), Times.Once);
    }

    [Test]
    public void Dispose_CalledTwice_ClearsCallbackOnlyOnce()
    {
        //Arrange
        var dockWidget = Create();

        //Act
        dockWidget.Dispose();
        dockWidget.Dispose();

        //Assert
        _shimMock.Verify(s => s.OnTopLevelChanged(Handle, IntPtr.Zero), Times.Once);
    }
}
