using DynamiteAndy.DynamiteToolbar.Interop.Bridge.Qt;
using DynamiteAndy.DynamiteToolbar.Interop.Obs;
using DynamiteAndy.DynamiteToolbar.Models;
using DynamiteAndy.DynamiteToolbar.Models.Events;
using DynamiteAndy.DynamiteToolbar.Services;
using DynamiteAndy.DynamiteToolbar.UI.Dock.Buttons;
using Moq;
using ObsInterop;

namespace DynamiteAndy.DynamiteToolbar.Tests.Unit.UI.Dock.Buttons;

[TestFixture]
public class StudioModeButtonTests : ButtonTestBase
{
    protected override string ExpectedTextKey => "Toolbar.Button.StudioMode";
    protected override IntPtr ExpectedButtonPtr => StudioBtnPtr;
    protected override void CreateForTextTest() => _ = Create(clearInvocations: false);

    private StudioModeButton Create(
        bool studioModeActive = false,
        bool visible = true,
        bool active = false,
        bool clearInvocations = true)
    {
        FrontendMock.Setup(f => f.PreviewProgramModeActive).Returns(studioModeActive);

        return CreateButton(
            () => new StudioModeButton(
                Context, LocaleMock.Object, FrontendMock.Object,
                PubSubMock.Object, BtnShimMock.Object, WidgetMock.Object),
            visible, active, clearInvocations);
    }

    private StudioModeButtonTestable CreateTestable(
        bool studioModeActive = false,
        bool visible = true,
        bool active = false,
        bool clearInvocations = true)
    {
        FrontendMock.Setup(f => f.PreviewProgramModeActive).Returns(studioModeActive);

        return CreateButton(
            () => new StudioModeButtonTestable(
                Context, LocaleMock.Object, FrontendMock.Object,
                PubSubMock.Object, BtnShimMock.Object, WidgetMock.Object),
            visible, active, clearInvocations);
    }

    // -----------------------------------------------------------------------
    // Construction
    // -----------------------------------------------------------------------

    [Test]
    public void Constructor_WhenInactive_IsUnchecked()
    {
        //Arrange + Act
        _ = Create(studioModeActive: false, clearInvocations: false);

        //Assert
        BtnShimMock.Verify(s => s.SetChecked(StudioBtnPtr, false), Times.Never);
    }

    [Test]
    public void Constructor_WhenActive_IsChecked()
    {
        //Arrange + Act
        _ = Create(studioModeActive: true, clearInvocations: false);

        //Assert
        BtnShimMock.Verify(s => s.SetChecked(StudioBtnPtr, true), Times.Once);
    }

    // -----------------------------------------------------------------------
    // OnObsFrontendEvent — state transitions
    // -----------------------------------------------------------------------

    [Test]
    public void OnObsFrontendEvent_StudioModeEnabled_ChecksButton()
    {
        //Arrange
        var button = Create();

        //Act
        button.OnObsFrontendEvent(new ObsFrontendEvent(obs_frontend_event.OBS_FRONTEND_EVENT_STUDIO_MODE_ENABLED));

        //Assert
        BtnShimMock.Verify(s => s.SetChecked(StudioBtnPtr, true), Times.Once);
    }

    [Test]
    public void OnObsFrontendEvent_StudioModeDisabled_UnchecksButton()
    {
        //Arrange — active: true reflects studio mode state; CreateButton clears invocations
        var button = Create(active: true);

        //Act
        button.OnObsFrontendEvent(new ObsFrontendEvent(obs_frontend_event.OBS_FRONTEND_EVENT_STUDIO_MODE_DISABLED));

        //Assert
        BtnShimMock.Verify(s => s.SetChecked(StudioBtnPtr, false), Times.Once);
    }

    [Test]
    public void OnObsFrontendEvent_UnrelatedEvent_DoesNotChangeState()
    {
        //Arrange — CreateButton clears BtnShimMock invocations after construction
        var button = Create();

        //Act
        button.OnObsFrontendEvent(new ObsFrontendEvent(obs_frontend_event.OBS_FRONTEND_EVENT_STREAMING_STARTED));

        //Assert
        BtnShimMock.Verify(s => s.SetChecked(It.IsAny<IntPtr>(), It.IsAny<bool>()), Times.Never);
    }

    // -----------------------------------------------------------------------
    // OnSettingsChangedEvent
    // -----------------------------------------------------------------------

    [Test]
    public void OnSettingsChangedEvent_HideStudioMode_HidesButton()
    {
        //Arrange
        var button = Create();

        //Act
        button.OnSettingsChangedEvent(new SettingsChangedEvent(new PluginSettings { HideStudioMode = true }));

        //Assert
        WidgetMock.Verify(s => s.SetVisible(StudioBtnPtr, false), Times.AtLeastOnce);
    }

    [Test]
    public void OnSettingsChangedEvent_ShowStudioMode_ShowsButton()
    {
        //Arrange — start hidden so a single settings event is enough to assert show
        var button = Create(visible: false);

        //Act
        button.OnSettingsChangedEvent(new SettingsChangedEvent(new PluginSettings { HideStudioMode = false }));

        //Assert
        WidgetMock.Verify(s => s.SetVisible(StudioBtnPtr, true), Times.Once);
    }

    // -----------------------------------------------------------------------
    // OnClicked — toggles studio mode
    // -----------------------------------------------------------------------

    [Test]
    public void OnClicked_WhenInactive_EnablesStudioMode()
    {
        //Arrange
        var button = CreateTestable(studioModeActive: false);

        //Act
        button.SimulateClick();

        //Assert
        FrontendMock.Verify(f => f.SetPreviewProgramMode(true), Times.Once);
    }

    [Test]
    public void OnClicked_WhenActive_DisablesStudioMode()
    {
        //Arrange
        var button = CreateTestable(studioModeActive: true);

        //Act
        button.SimulateClick();

        //Assert
        FrontendMock.Verify(f => f.SetPreviewProgramMode(false), Times.Once);
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
        PubSubMock.Verify(p => p.Unsubscribe(It.IsAny<Action<ObsFrontendEvent>>()), Times.Once);
        PubSubMock.Verify(p => p.Unsubscribe(It.IsAny<Action<SettingsChangedEvent>>()), Times.Once);
    }

    // -----------------------------------------------------------------------
    // Testable subclass — exposes OnClicked
    // -----------------------------------------------------------------------

    private sealed class StudioModeButtonTestable(
        IQtControlContext ctx,
        ILocalizationService locale,
        IObsFrontendApi frontend,
        IPubSubService pubSub,
        IQAbstractButtonShim btnShim,
        IQWidgetShim widgetShim)
        : StudioModeButton(ctx, locale, frontend, pubSub, btnShim, widgetShim)
    {
        public void SimulateClick() => OnClicked(false);
    }
}
