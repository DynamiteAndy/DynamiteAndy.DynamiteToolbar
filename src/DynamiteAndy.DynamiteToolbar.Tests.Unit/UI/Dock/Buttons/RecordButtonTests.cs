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
public class RecordButtonTests : ButtonTestBase
{
    protected override string ExpectedTextKey => "Toolbar.Button.Record.Start";
    protected override IntPtr ExpectedButtonPtr => RecordBtnPtr;
    protected override void CreateForTextTest() => _ = Create(clearInvocations: false);

    // Constructor sets: Checked = isActive || isPaused, Visible = !isPaused
    // active and visible mirror that so CreateButton doesn't fight the constructor.
    private RecordButton Create(
        bool recordingActive = false,
        bool recordingPaused = false,
        bool clearInvocations = true)
    {
        FrontendMock.Setup(f => f.RecordingActive).Returns(recordingActive);
        FrontendMock.Setup(f => f.RecordingPaused).Returns(recordingPaused);

        return CreateButton(
            () => new RecordButton(
                Context, LocaleMock.Object, FrontendMock.Object,
                MsgBoxMock.Object, PubSubMock.Object, BtnShimMock.Object, WidgetMock.Object),
            visible: !recordingPaused,
            active: recordingActive || recordingPaused,
            clearInvocations);
    }

    private RecordButtonTestable CreateTestable(
        bool recordingActive = false,
        bool recordingPaused = false)
    {
        FrontendMock.Setup(f => f.RecordingActive).Returns(recordingActive);
        FrontendMock.Setup(f => f.RecordingPaused).Returns(recordingPaused);

        return CreateButton(
            () => new RecordButtonTestable(
                Context, LocaleMock.Object, FrontendMock.Object,
                MsgBoxMock.Object, PubSubMock.Object, BtnShimMock.Object, WidgetMock.Object),
            visible: !recordingPaused,
            active: recordingActive || recordingPaused);
    }

    // -----------------------------------------------------------------------
    // Construction
    // -----------------------------------------------------------------------

    // RecordButton sets a Tooltip rather than Text, so the base Constructor_SetsText
    // test is overridden here.
    [Test]
    public override void Constructor_SetsText()
    {
        //Arrange + Act
        _ = Create(clearInvocations: false);

        //Assert — button uses tooltip not text
        WidgetMock.Verify(s => s.SetTooltip(RecordBtnPtr, "Toolbar.Button.Record.Start"), Times.Once);
    }

    [Test]
    public void Constructor_WhenIdle_IsVisible()
    {
        //Arrange + Act — clearInvocations:false so constructor shim calls are visible
        _ = Create(clearInvocations: false);

        //Assert — Visible defaults to true, constructor computes true&&true=true, guard blocks
        WidgetMock.Verify(s => s.SetVisible(RecordBtnPtr, It.IsAny<bool>()), Times.Never);
    }

    [Test]
    public void Constructor_WhenActiveNotPaused_SetsStopTooltipAndChecked()
    {
        //Arrange + Act
        _ = Create(recordingActive: true, clearInvocations: false);

        //Assert
        BtnShimMock.Verify(s => s.SetChecked(RecordBtnPtr, true), Times.Once);
        WidgetMock.Verify(s => s.SetTooltip(RecordBtnPtr, "Toolbar.Button.Record.Stop"), Times.Once);
        WidgetMock.Verify(s => s.SetVisible(RecordBtnPtr, It.IsAny<bool>()), Times.Never);
    }

    [Test]
    public void Constructor_WhenPaused_IsHidden()
    {
        //Arrange + Act
        _ = Create(recordingPaused: true, clearInvocations: false);

        //Assert — Visible = true && !isPaused = false → fires SetVisible(false)
        WidgetMock.Verify(s => s.SetVisible(RecordBtnPtr, false), Times.Once);
    }

    // -----------------------------------------------------------------------
    // OnObsFrontendEvent — state transitions
    // -----------------------------------------------------------------------

    [Test]
    public void OnObsFrontendEvent_RecordingStarted_ChecksButton()
    {
        //Arrange — idle: Checked=false, _visibilityFromRecording=true
        var button = Create();

        //Act
        button.OnObsFrontendEvent(new ObsFrontendEvent(obs_frontend_event.OBS_FRONTEND_EVENT_RECORDING_STARTED));

        //Assert — isActive(true) != Checked(false) → SetChecked(true)
        BtnShimMock.Verify(s => s.SetChecked(RecordBtnPtr, true), Times.Once);
    }

    [Test]
    public void OnObsFrontendEvent_RecordingPaused_HidesButton()
    {
        //Arrange — active: Checked=true, _visibilityFromRecording=true, Visible=true
        var button = Create(recordingActive: true);

        //Act
        button.OnObsFrontendEvent(new ObsFrontendEvent(obs_frontend_event.OBS_FRONTEND_EVENT_RECORDING_PAUSED));

        //Assert — isVisible(false) != _visibilityFromRecording(true) → Visible flips true→false
        WidgetMock.Verify(s => s.SetVisible(RecordBtnPtr, false), Times.Once);
    }

    [Test]
    public void OnObsFrontendEvent_RecordingStopping_ChecksButtonAndShowsButton()
    {
        //Arrange — idle: Checked=false, _visibilityFromRecording=true
        var button = Create();

        //Act
        button.OnObsFrontendEvent(new ObsFrontendEvent(obs_frontend_event.OBS_FRONTEND_EVENT_RECORDING_STOPPING));

        //Assert — isActive(true) != Checked(false) → SetChecked(true); isVisible(true) == _visibilityFromRecording(true) → no visibility change
        BtnShimMock.Verify(s => s.SetChecked(RecordBtnPtr, true), Times.Once);
        WidgetMock.Verify(s => s.SetVisible(RecordBtnPtr, It.IsAny<bool>()), Times.Never);
    }

    [Test]
    public void OnObsFrontendEvent_RecordingUnpaused_ShowsButton()
    {
        //Arrange — paused: Checked=true, _visibilityFromRecording=false, Visible=false
        var button = Create(recordingPaused: true);

        //Act
        button.OnObsFrontendEvent(new ObsFrontendEvent(obs_frontend_event.OBS_FRONTEND_EVENT_RECORDING_UNPAUSED));

        //Assert — isVisible(true) != _visibilityFromRecording(false) → Visible flips false→true
        WidgetMock.Verify(s => s.SetVisible(RecordBtnPtr, true), Times.Once);
    }

    [Test]
    public void OnObsFrontendEvent_RecordingStopped_UnchecksAndSetsStartTooltip()
    {
        //Arrange — active: Checked=true, _visibilityFromRecording=true
        var button = Create(recordingActive: true);

        //Act
        button.OnObsFrontendEvent(new ObsFrontendEvent(obs_frontend_event.OBS_FRONTEND_EVENT_RECORDING_STOPPED));

        //Assert — isActive(false) != Checked(true) → SetChecked(false) + tooltip update
        BtnShimMock.Verify(s => s.SetChecked(RecordBtnPtr, false), Times.Once);
        WidgetMock.Verify(s => s.SetTooltip(RecordBtnPtr, "Toolbar.Button.Record.Start"), Times.Once);
    }

    [Test]
    public void OnObsFrontendEvent_UnrelatedEvent_DoesNotChangeState()
    {
        //Arrange
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
    public void OnSettingsChangedEvent_HideStartRecording_HidesButton()
    {
        //Arrange — idle so button is visible
        var button = Create();

        //Act
        button.OnSettingsChangedEvent(new SettingsChangedEvent(new PluginSettings { HideStartRecording = true }));

        //Assert
        WidgetMock.Verify(s => s.SetVisible(RecordBtnPtr, false), Times.AtLeastOnce);
    }

    [Test]
    public void OnSettingsChangedEvent_ShowStartRecording_ShowsButton()
    {
        //Arrange — hide via setting first, then clear
        var button = Create();
        button.OnSettingsChangedEvent(new SettingsChangedEvent(new PluginSettings { HideStartRecording = true }));
        WidgetMock.Invocations.Clear();

        //Act
        button.OnSettingsChangedEvent(new SettingsChangedEvent(new PluginSettings { HideStartRecording = false }));

        //Assert
        WidgetMock.Verify(s => s.SetVisible(RecordBtnPtr, true), Times.Once);
    }

    // -----------------------------------------------------------------------
    // OnClicked — start recording when idle
    // -----------------------------------------------------------------------

    [Test]
    public void OnClicked_WhenIdle_StartsRecording()
    {
        //Arrange
        var button = CreateTestable();

        //Act
        button.SimulateClick();

        //Assert
        FrontendMock.Verify(f => f.RecordingStart(), Times.Once);
    }

    // -----------------------------------------------------------------------
    // OnClicked — stop path with confirmation
    // -----------------------------------------------------------------------

    [Test]
    public void OnClicked_WhenActive_WithModalEnabled_PausesBeforeShowing()
    {
        //Arrange
        MsgBoxMock.Setup(m => m.ShowConfirmation(It.IsAny<string>(), It.IsAny<string>())).Returns(true);
        var button = CreateTestable(recordingActive: true);

        //Act
        button.SimulateClick();

        //Assert
        FrontendMock.Verify(f => f.RecordingPause(true), Times.Once);
    }

    [Test]
    public void OnClicked_WhenActive_UserConfirms_StopsRecording()
    {
        //Arrange
        MsgBoxMock.Setup(m => m.ShowConfirmation(It.IsAny<string>(), It.IsAny<string>())).Returns(true);
        var button = CreateTestable(recordingActive: true);

        //Act
        button.SimulateClick();

        //Assert
        FrontendMock.Verify(f => f.RecordingStop(), Times.Once);
        FrontendMock.Verify(f => f.RecordingPause(false), Times.Never);
    }

    [Test]
    public void OnClicked_WhenActive_UserCancels_ResumesRecording()
    {
        //Arrange
        MsgBoxMock.Setup(m => m.ShowConfirmation(It.IsAny<string>(), It.IsAny<string>())).Returns(false);
        var button = CreateTestable(recordingActive: true);

        //Act
        button.SimulateClick();

        //Assert
        FrontendMock.Verify(f => f.RecordingStop(), Times.Never);
        FrontendMock.Verify(f => f.RecordingPause(false), Times.Once);
    }

    [Test]
    public void OnClicked_WhenActive_ModalDisabled_StopsWithoutPausingOrModal()
    {
        //Arrange
        var button = CreateTestable(recordingActive: true);
        button.OnSettingsChangedEvent(new SettingsChangedEvent(new PluginSettings { HideStopRecordingModal = true }));

        //Act
        button.SimulateClick();

        //Assert
        FrontendMock.Verify(f => f.RecordingPause(It.IsAny<bool>()), Times.Never);
        MsgBoxMock.Verify(m => m.ShowConfirmation(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        FrontendMock.Verify(f => f.RecordingStop(), Times.Once);
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

    private sealed class RecordButtonTestable(
        IQtControlContext ctx,
        ILocalizationService locale,
        IObsFrontendApi frontend,
        IQMessageBoxShim msgBox,
        IPubSubService pubSub,
        IQAbstractButtonShim btnShim,
        IQWidgetShim widgetShim)
        : RecordButton(ctx, locale, frontend, msgBox, pubSub, btnShim, widgetShim)
    {
        public void SimulateClick() => OnClicked(false);
    }
}
