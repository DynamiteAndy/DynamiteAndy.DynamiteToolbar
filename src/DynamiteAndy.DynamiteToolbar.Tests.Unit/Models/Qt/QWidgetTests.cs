using DynamiteAndy.DynamiteToolbar.Interop.Bridge.Qt;
using DynamiteAndy.DynamiteToolbar.Models.Qt;
using Moq;

namespace DynamiteAndy.DynamiteToolbar.Tests.Unit.Models.Qt;

[TestFixture]
public class QWidgetTests
{
    private Mock<IQWidgetShim> _shimMock = null!;
    private static readonly IntPtr Handle = new(0x100);

    [SetUp]
    public void SetUp() => _shimMock = new Mock<IQWidgetShim>();

    private QWidget Create() => new(Handle, _shimMock.Object);

    // -----------------------------------------------------------------------
    // Default state
    // -----------------------------------------------------------------------

    [Test]
    public void DefaultVisible_IsTrue()
    {
        //Arrange + Act
        var widget = Create();

        //Assert
        Assert.That(widget.Visible, Is.True);
    }

    [Test]
    public void DefaultTooltip_IsNull()
    {
        //Arrange + Act
        var widget = Create();

        //Assert
        Assert.That(widget.Tooltip, Is.Null);
    }

    // -----------------------------------------------------------------------
    // Visible
    // -----------------------------------------------------------------------

    [Test]
    public void SetVisible_False_CallsShim()
    {
        //Arrange
        var widget = Create();

        //Act
        widget.Visible = false;

        //Assert
        _shimMock.Verify(s => s.SetVisible(Handle, false), Times.Once);
    }

    [Test]
    public void SetVisible_True_WhenAlreadyTrue_DoesNotCallShim()
    {
        //Arrange — default is true
        var widget = Create();

        //Act
        widget.Visible = true;

        //Assert
        _shimMock.Verify(s => s.SetVisible(It.IsAny<IntPtr>(), It.IsAny<bool>()), Times.Never);
    }

    [Test]
    public void SetVisible_SameValueTwice_CallsShimOnlyOnce()
    {
        //Arrange
        var widget = Create();

        //Act
        widget.Visible = false;
        widget.Visible = false;

        //Assert
        _shimMock.Verify(s => s.SetVisible(Handle, false), Times.Once);
    }

    [Test]
    public void SetVisible_ToggleBackAndForth_CallsShimEachTime()
    {
        //Arrange
        var widget = Create();

        //Act
        widget.Visible = false;
        widget.Visible = true;
        widget.Visible = false;

        //Assert
        _shimMock.Verify(s => s.SetVisible(Handle, false), Times.Exactly(2));
        _shimMock.Verify(s => s.SetVisible(Handle, true), Times.Once);
    }

    // -----------------------------------------------------------------------
    // Tooltip
    // -----------------------------------------------------------------------

    [Test]
    public void SetTooltip_CallsShim()
    {
        //Arrange
        var widget = Create();

        //Act
        widget.Tooltip = "hint";

        //Assert
        _shimMock.Verify(s => s.SetTooltip(Handle, "hint"), Times.Once);
    }

    [Test]
    public void SetTooltip_SameValueTwice_CallsShimOnlyOnce()
    {
        //Arrange
        var widget = Create();

        //Act
        widget.Tooltip = "same";
        widget.Tooltip = "same";

        //Assert
        _shimMock.Verify(s => s.SetTooltip(Handle, "same"), Times.Once);
    }

    [Test]
    public void SetTooltip_Null_DoesNotCallShim()
    {
        //Arrange
        var widget = Create();

        //Act
        widget.Tooltip = null;

        //Assert
        _shimMock.Verify(s => s.SetTooltip(It.IsAny<IntPtr>(), It.IsAny<string>()), Times.Never);
    }

    [Test]
    public void SetTooltip_AfterNull_CallsShim()
    {
        //Arrange — null first so the field starts null
        var widget = Create();
        widget.Tooltip = null;

        //Act
        widget.Tooltip = "now set";

        //Assert
        _shimMock.Verify(s => s.SetTooltip(Handle, "now set"), Times.Once);
    }
}
