using DynamiteAndy.DynamiteToolbar.Interop.Bridge.Qt;
using DynamiteAndy.DynamiteToolbar.Models;
using DynamiteAndy.DynamiteToolbar.Models.Events;
using DynamiteAndy.DynamiteToolbar.Services;
using DynamiteAndy.DynamiteToolbar.UI.Dock.Buttons;
using Moq;

namespace DynamiteAndy.DynamiteToolbar.Tests.Unit.UI.Dock.Buttons;

[TestFixture]
public class FloatButtonTests : ButtonTestBase
{
    protected override string ExpectedTextKey => "Toolbar.Button.Float";
    protected override IntPtr ExpectedButtonPtr => FloatBtnPtr;
    protected override void CreateForTextTest() => _ = Create(clearInvocations: false);

    private FloatButton Create(bool visible = true, bool active = false, bool clearInvocations = true) => CreateButton(
        () => new FloatButton(
            Context, LocaleMock.Object, PubSubMock.Object,
            DockWidgetMock.Object, BtnShimMock.Object, WidgetMock.Object),
        visible, active, clearInvocations);

    /// <summary>
    /// Captures the TopLevelChangedEvent handler that FloatButton subscribes with
    /// and returns a helper that invokes it directly.
    /// </summary>
    private (FloatButton button, Action<TopLevelChangedEvent> raiseTopLevel) CreateWithCapture(
        bool visible = true, bool active = false)
    {
        Action<TopLevelChangedEvent>? captured = null;
        PubSubMock
            .Setup(p => p.Subscribe<TopLevelChangedEvent>(It.IsAny<Action<TopLevelChangedEvent>>()))
            .Callback<Action<TopLevelChangedEvent>>(h => captured = h);

        var button = Create(visible, active);

        Assert.That(captured, Is.Not.Null, "FloatButton did not subscribe to TopLevelChangedEvent");
        return (button, captured!);
    }

    // -----------------------------------------------------------------------
    // Construction
    // -----------------------------------------------------------------------

    [Test]
    public void Constructor_SubscribesToTopLevelChangedAndSettingsChanged()
    {
        //Arrange + Act
        _ = Create();

        //Assert
        PubSubMock.Verify(p => p.Subscribe<TopLevelChangedEvent>(It.IsAny<Action<TopLevelChangedEvent>>()), Times.Once);
        PubSubMock.Verify(p => p.Subscribe<SettingsChangedEvent>(It.IsAny<Action<SettingsChangedEvent>>()), Times.Once);
    }

    // -----------------------------------------------------------------------
    // TopLevelChangedEvent — driven via captured handler
    // -----------------------------------------------------------------------

    [Test]
    public void OnTopLevelChangedEvent_Floating_ChecksButton()
    {
        //Arrange
        var (button, raiseTopLevel) = CreateWithCapture();

        //Act
        raiseTopLevel(new TopLevelChangedEvent(true));

        //Assert
        BtnShimMock.Verify(s => s.SetChecked(FloatBtnPtr, true), Times.Once);
    }

    [Test]
    public void OnTopLevelChangedEvent_Floating_HidesButton()
    {
        //Arrange
        var (button, raiseTopLevel) = CreateWithCapture();

        //Act
        raiseTopLevel(new TopLevelChangedEvent(true));

        //Assert
        WidgetMock.Verify(s => s.SetVisible(FloatBtnPtr, false), Times.AtLeastOnce);
    }

    [Test]
    public void OnTopLevelChangedEvent_NotFloating_ShowsButton()
    {
        //Arrange — start hidden (as if we were floating) so un-floating can assert show
        var (button, raiseTopLevel) = CreateWithCapture(visible: false);

        //Act
        raiseTopLevel(new TopLevelChangedEvent(false));

        //Assert
        WidgetMock.Verify(s => s.SetVisible(FloatBtnPtr, true), Times.Once);
    }

    [Test]
    public void OnTopLevelChangedEvent_NotFloating_UnchecksButton()
    {
        //Arrange — start checked (as if we were floating) so un-floating can assert uncheck
        var (button, raiseTopLevel) = CreateWithCapture(active: true);

        //Act
        raiseTopLevel(new TopLevelChangedEvent(false));

        //Assert
        BtnShimMock.Verify(s => s.SetChecked(FloatBtnPtr, false), Times.Once);
    }

    // -----------------------------------------------------------------------
    // OnSettingsChangedEvent — public, callable directly
    // -----------------------------------------------------------------------

    [Test]
    public void OnSettingsChangedEvent_HideFloat_HidesButton()
    {
        //Arrange
        var button = Create();

        //Act
        button.OnSettingsChangedEvent(new SettingsChangedEvent(new PluginSettings { HideFloat = true }));

        //Assert
        WidgetMock.Verify(s => s.SetVisible(FloatBtnPtr, false), Times.AtLeastOnce);
    }

    [Test]
    public void OnSettingsChangedEvent_ShowFloat_AndNotFloating_ShowsButton()
    {
        //Arrange — start hidden so a single settings event is enough to assert show
        var button = Create(visible: false);

        //Act
        button.OnSettingsChangedEvent(new SettingsChangedEvent(new PluginSettings { HideFloat = false }));

        //Assert
        WidgetMock.Verify(s => s.SetVisible(FloatBtnPtr, true), Times.Once);
    }

    // -----------------------------------------------------------------------
    // Combined gate: hidden when floating OR setting hides
    // -----------------------------------------------------------------------

    [Test]
    public void Visibility_HiddenWhenFloating_EvenIfSettingAllows()
    {
        //Arrange — visible: true means the setting already allows it; no setup event needed
        var (button, raiseTopLevel) = CreateWithCapture(visible: true);

        //Act
        raiseTopLevel(new TopLevelChangedEvent(true)); // floating

        //Assert
        WidgetMock.Verify(s => s.SetVisible(FloatBtnPtr, false), Times.Once);
        WidgetMock.Verify(s => s.SetVisible(FloatBtnPtr, true), Times.Never);
    }

    [Test]
    public void Visibility_HiddenWhenSettingHides_EvenIfNotFloating()
    {
        //Arrange — start visible (not floating)
        var button = Create(visible: true);

        //Act
        button.OnSettingsChangedEvent(new SettingsChangedEvent(new PluginSettings { HideFloat = true }));

        //Assert
        WidgetMock.Verify(s => s.SetVisible(FloatBtnPtr, false), Times.Once);
        WidgetMock.Verify(s => s.SetVisible(FloatBtnPtr, true), Times.Never);
    }

    // -----------------------------------------------------------------------
    // OnClicked — calls SetFloating(true)
    // -----------------------------------------------------------------------

    [Test]
    public void OnClicked_CallsSetFloating_True()
    {
        //Arrange
        var button = new FloatButtonTestable(
            Context, LocaleMock.Object, PubSubMock.Object,
            DockWidgetMock.Object, BtnShimMock.Object, WidgetMock.Object);

        //Act
        button.SimulateClick();

        //Assert
        DockWidgetMock.Verify(s => s.SetFloating(DockPtr, true), Times.Once);
    }

    // -----------------------------------------------------------------------
    // Dispose
    // -----------------------------------------------------------------------

    [Test]
    public void Dispose_UnsubscribesFromEvents()
    {
        //Arrange
        var button = Create();

        //Act
        button.Dispose();

        //Assert
        PubSubMock.Verify(p => p.Unsubscribe(It.IsAny<Action<TopLevelChangedEvent>>()), Times.Once);
        PubSubMock.Verify(p => p.Unsubscribe(It.IsAny<Action<SettingsChangedEvent>>()), Times.Once);
    }

    // -----------------------------------------------------------------------
    // Testable subclass to expose OnClicked
    // -----------------------------------------------------------------------

    private sealed class FloatButtonTestable(
        IQtControlContext ctx,
        ILocalizationService locale,
        IPubSubService pubSub,
        IQDockWidgetShim dockShim,
        IQAbstractButtonShim btnShim,
        IQWidgetShim widgetShim)
        : FloatButton(ctx, locale, pubSub, dockShim, btnShim, widgetShim)
    {
        public void SimulateClick() => OnClicked(false);
    }
}
