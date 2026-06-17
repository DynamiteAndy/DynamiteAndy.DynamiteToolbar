using DynamiteAndy.DynamiteToolbar.Interop.Bridge.Qt;
using DynamiteAndy.DynamiteToolbar.Models;
using DynamiteAndy.DynamiteToolbar.Models.Shim;
using Moq;

namespace DynamiteAndy.DynamiteToolbar.Tests.Unit.Models;

[TestFixture]
public class LabelGroupTests
{
    private Mock<IQLabelShim> _labelMock = null!;
    private Mock<IQWidgetShim> _widgetMock = null!;

    private static readonly IntPtr NamePtr = new(0x01);
    private static readonly IntPtr DelimPtr = new(0x02);
    private static readonly IntPtr ValuePtr = new(0x03);
    private static readonly IntPtr SeparatorPtr = new(0x04);

    [SetUp]
    public void SetUp()
    {
        _labelMock = new Mock<IQLabelShim>();
        _widgetMock = new Mock<IQWidgetShim>();
    }

    private LabelGroup Create(
        bool withSeparator = true,
        bool visible = true,
        bool clearInvocations = true)
    {
        var group = new LabelGroup(
            new DockFrameMonitorLabel
            {
                NameLblPtr = NamePtr,
                DelimiterLblPtr = DelimPtr,
                ValueLblPtr = ValuePtr,
                SeparatorLblPtr = withSeparator ? SeparatorPtr : IntPtr.Zero,
            },
            _labelMock.Object, _widgetMock.Object)
        {
            Visible = visible
        };

        if (clearInvocations)
        {
            _widgetMock.Invocations.Clear();
        }

        return group;
    }

    // -----------------------------------------------------------------------
    // Default state
    // -----------------------------------------------------------------------

    [Test]
    public void DefaultVisible_IsTrue()
    {
        //Arrange + Act
        var group = Create(clearInvocations: false);

        //Assert
        Assert.That(group.Visible, Is.True);
    }

    [Test]
    public void WithoutSeparator_SeparatorIsNull()
    {
        //Arrange + Act
        var group = Create(withSeparator: false, clearInvocations: false);

        //Assert
        Assert.That(group.Separator, Is.Null);
    }

    [Test]
    public void WithSeparator_SeparatorIsNotNull()
    {
        //Arrange + Act
        var group = Create(clearInvocations: false);

        //Assert
        Assert.That(group.Separator, Is.Not.Null);
    }

    // -----------------------------------------------------------------------
    // Visible setter
    // -----------------------------------------------------------------------

    [Test]
    public void SetVisible_False_HidesAllChildren()
    {
        //Arrange
        var group = Create();

        //Act
        group.Visible = false;

        //Assert
        _widgetMock.Verify(s => s.SetVisible(NamePtr, false), Times.Once);
        _widgetMock.Verify(s => s.SetVisible(DelimPtr, false), Times.Once);
        _widgetMock.Verify(s => s.SetVisible(ValuePtr, false), Times.Once);
        _widgetMock.Verify(s => s.SetVisible(SeparatorPtr, false), Times.Once);
    }

    [Test]
    public void SetVisible_True_ShowsAllChildren()
    {
        //Arrange — start hidden so flipping to true triggers shim calls
        var group = Create(visible: false);

        //Act
        group.Visible = true;

        //Assert
        _widgetMock.Verify(s => s.SetVisible(NamePtr, true), Times.Once);
        _widgetMock.Verify(s => s.SetVisible(DelimPtr, true), Times.Once);
        _widgetMock.Verify(s => s.SetVisible(ValuePtr, true), Times.Once);
        _widgetMock.Verify(s => s.SetVisible(SeparatorPtr, true), Times.Once);
    }

    [Test]
    public void SetVisible_SameValueTwice_DoesNotPropagateAgain()
    {
        //Arrange — start hidden
        var group = Create(visible: false);

        //Act — set false again; guard should block
        group.Visible = false;

        //Assert
        _widgetMock.Verify(s => s.SetVisible(It.IsAny<IntPtr>(), It.IsAny<bool>()), Times.Never);
    }

    [Test]
    public void SetVisible_WithoutSeparator_DoesNotTouchSeparatorPtr()
    {
        //Arrange
        var group = Create(withSeparator: false);

        //Act
        group.Visible = false;

        //Assert
        _widgetMock.Verify(s => s.SetVisible(SeparatorPtr, It.IsAny<bool>()), Times.Never);
    }

    [Test]
    public void SetVisible_RaisesVisibilityChangedEvent()
    {
        //Arrange
        var group = Create();
        int count = 0;
        group.VisibilityChanged += () => count++;

        //Act
        group.Visible = false;

        //Assert
        Assert.That(count, Is.EqualTo(1));
    }

    [Test]
    public void SetVisible_SameValue_DoesNotRaiseEvent()
    {
        //Arrange
        var group = Create();
        int count = 0;
        group.VisibilityChanged += () => count++;

        //Act — already true, no change
        group.Visible = true;

        //Assert
        Assert.That(count, Is.Zero);
    }

    // -----------------------------------------------------------------------
    // UpdateSeparator
    // -----------------------------------------------------------------------

    [Test]
    public void UpdateSeparator_WhenNotLast_ShowsSeparator()
    {
        //Arrange — hide separator first via isLast=true, then clear so we isolate the act
        var group = Create();
        group.UpdateSeparator(isLastVisible: true);
        _widgetMock.Invocations.Clear();

        //Act
        group.UpdateSeparator(isLastVisible: false);

        //Assert — Separator.Visible flips false→true
        _widgetMock.Verify(s => s.SetVisible(SeparatorPtr, true), Times.Once);
    }

    [Test]
    public void UpdateSeparator_WhenLast_HidesSeparator()
    {
        //Arrange — visible=true (default), separator starts true; isLast forces it hidden
        var group = Create();

        //Act
        group.UpdateSeparator(isLastVisible: true);

        //Assert — Separator.Visible flips true→false
        _widgetMock.Verify(s => s.SetVisible(SeparatorPtr, false), Times.Once);
    }

    [Test]
    public void UpdateSeparator_WithoutSeparator_DoesNotThrow()
    {
        //Arrange
        var group = Create(withSeparator: false);

        //Act + Assert
        Assert.DoesNotThrow(() => group.UpdateSeparator(isLastVisible: false));
    }

    [Test]
    public void UpdateSeparator_WhenGroupHidden_SeparatorRemainsHidden()
    {
        //Arrange — start hidden; separator is already false
        var group = Create(visible: false);

        //Act — even "not last" should not show separator when group is hidden
        group.UpdateSeparator(isLastVisible: false);

        //Assert — guard blocks redundant SetVisible(false); separator is never touched
        _widgetMock.Verify(s => s.SetVisible(SeparatorPtr, It.IsAny<bool>()), Times.Never);
    }

    // -----------------------------------------------------------------------
    // Dispose
    // -----------------------------------------------------------------------

    [Test]
    public void Dispose_CalledTwice_DoesNotThrow()
    {
        //Arrange
        var group = Create();

        //Act + Assert
        Assert.DoesNotThrow(() => { group.Dispose(); group.Dispose(); });
    }
}
