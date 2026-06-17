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
public class RecordPausedButtonTests : ButtonTestBase
{
    // RecordPausedButton sets a Tooltip rather than Text, so the base
    // Constructor_SetsText test is overridden here.
    protected override string ExpectedTextKey => "Toolbar.Button.Record.Resume";
    protected override IntPtr ExpectedButtonPtr => RecPausedPtr;
    protected override void CreateForTextTest() => _ = Create(clearInvocations: false);

    [Test]
    public override void Constructor_SetsText()
    {
        //Arrange + Act
        _ = Create(clearInvocations: false);

        //Assert — this button sets a tooltip, not button text
        WidgetMock.Verify(s => s.SetTooltip(RecPausedPtr, "Toolbar.Button.Record.Paused"), Times.Once);
    }

    // Constructor: Visible = recordingPaused (via UpdateVisibility), Checked = recordingPaused
    // visible and active mirror that so CreateButton doesn't fight the constructor.
    private RecordPausedButton Create(
        bool recordingPaused = false,
        bool clearInvocations = true)
    {
        FrontendMock.Setup(f => f.RecordingPaused).Returns(recordingPaused);

        return CreateButton(
            () => new RecordPausedButton(
                Context, LocaleMock.Object, FrontendMock.Object,
                PubSubMock.Object, BtnShimMock.Object, WidgetMock.Object),
            visible: recordingPaused, active: recordingPaused, clearInvocations);
    }

    // -----------------------------------------------------------------------
    // Construction
    // -----------------------------------------------------------------------

    [Test]
    public void Constructor_WhenNotPaused_IsHidden()
    {
        //Arrange + Act
        _ = Create(recordingPaused: false, clearInvocations: false);

        //Assert — UpdateVisibility() → Visible = true && false = false
        WidgetMock.Verify(s => s.SetVisible(RecPausedPtr, false), Times.Once);
    }

    [Test]
    public void Constructor_WhenPaused_DoesNotCallSetVisible()
    {
        //Arrange + Act — Visible defaults to true; constructor computes true && true = true
        // which the guard blocks, so SetVisible is never called
        _ = Create(recordingPaused: true, clearInvocations: false);

        //Assert
        WidgetMock.Verify(s => s.SetVisible(RecPausedPtr, It.IsAny<bool>()), Times.Never);
    }

    // -----------------------------------------------------------------------
    // OnObsFrontendEvent — state transitions
    // -----------------------------------------------------------------------

    [Test]
    public void OnObsFrontendEvent_RecordingPaused_ShowsButton()
    {
        //Arrange — not paused: Visible=false, Checked=false; CreateButton clears invocations
        var button = Create(recordingPaused: false);

        //Act
        button.OnObsFrontendEvent(new ObsFrontendEvent(obs_frontend_event.OBS_FRONTEND_EVENT_RECORDING_PAUSED));

        //Assert — isActive(true) != Checked(false) → Visible flips false→true
        WidgetMock.Verify(s => s.SetVisible(RecPausedPtr, true), Times.Once);
    }

    [Test]
    public void OnObsFrontendEvent_RecordingUnpaused_HidesButton()
    {
        //Arrange — paused: Visible=true (guard blocked), Checked=true; CreateButton clears invocations
        var button = Create(recordingPaused: true);

        //Act
        button.OnObsFrontendEvent(new ObsFrontendEvent(obs_frontend_event.OBS_FRONTEND_EVENT_RECORDING_UNPAUSED));

        //Assert — isActive(false) != Checked(true) → Visible flips true→false
        WidgetMock.Verify(s => s.SetVisible(RecPausedPtr, false), Times.Once);
    }

    [Test]
    public void OnObsFrontendEvent_RecordingStopped_HidesButton()
    {
        //Arrange — paused: Visible=true (guard blocked), Checked=true; CreateButton clears invocations
        var button = Create(recordingPaused: true);

        //Act
        button.OnObsFrontendEvent(new ObsFrontendEvent(obs_frontend_event.OBS_FRONTEND_EVENT_RECORDING_STOPPED));

        //Assert — isActive(false) != Checked(true) → Visible flips true→false
        WidgetMock.Verify(s => s.SetVisible(RecPausedPtr, false), Times.Once);
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
        WidgetMock.Verify(s => s.SetVisible(It.IsAny<IntPtr>(), It.IsAny<bool>()), Times.Never);
    }

    // -----------------------------------------------------------------------
    // OnSettingsChangedEvent
    // -----------------------------------------------------------------------

    [Test]
    public void OnSettingsChangedEvent_HideStartRecording_HidesButton()
    {
        //Arrange — paused so button is visible
        var button = Create(recordingPaused: true);

        //Act
        button.OnSettingsChangedEvent(new SettingsChangedEvent(new PluginSettings { HideStartRecording = true }));

        //Assert
        WidgetMock.Verify(s => s.SetVisible(RecPausedPtr, false), Times.AtLeastOnce);
    }

    [Test]
    public void OnSettingsChangedEvent_ShowStartRecording_WhilePaused_ShowsButton()
    {
        //Arrange — paused + hidden via setting
        var button = Create(recordingPaused: true);
        button.OnSettingsChangedEvent(new SettingsChangedEvent(new PluginSettings { HideStartRecording = true }));
        WidgetMock.Invocations.Clear();

        //Act
        button.OnSettingsChangedEvent(new SettingsChangedEvent(new PluginSettings { HideStartRecording = false }));

        //Assert
        WidgetMock.Verify(s => s.SetVisible(RecPausedPtr, true), Times.Once);
    }

    [Test]
    public void OnSettingsChangedEvent_ShowStartRecording_WhileNotPaused_RemainsHidden()
    {
        //Arrange — not paused so button is already hidden via recording state
        var button = Create(recordingPaused: false);
        button.OnSettingsChangedEvent(new SettingsChangedEvent(new PluginSettings { HideStartRecording = true }));
        WidgetMock.Invocations.Clear();

        //Act
        button.OnSettingsChangedEvent(new SettingsChangedEvent(new PluginSettings { HideStartRecording = false }));

        //Assert — not paused so stays hidden
        WidgetMock.Verify(s => s.SetVisible(RecPausedPtr, true), Times.Never);
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
}
