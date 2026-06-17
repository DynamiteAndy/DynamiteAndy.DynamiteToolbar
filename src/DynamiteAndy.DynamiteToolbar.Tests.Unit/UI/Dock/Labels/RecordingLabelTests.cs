using DynamiteAndy.DynamiteToolbar.Models.Events;
using DynamiteAndy.DynamiteToolbar.Models.Shim;
using DynamiteAndy.DynamiteToolbar.UI.Dock.Labels;
using Moq;
using ObsInterop;
using PluginSettings = DynamiteAndy.DynamiteToolbar.Models.PluginSettings;

namespace DynamiteAndy.DynamiteToolbar.Tests.Unit.UI.Dock.Labels;

[TestFixture]
public class RecordingLabelTests : LabelTestBase
{
    protected override PluginDock BuildPluginDock() => new()
    {
        Monitor = new DockFrameMonitor { Recording = MakeDockFrameMonitorLabel() }
    };

    protected override string ExpectedNameKey => "Toolbar.Label.Recording";

    protected override void CreateForNameLabelTest() => _ = Create(clearInvocations: false);

    private RecordingLabel Create(
        bool recordingActive = false,
        bool recordingPaused = false,
        bool visible = true,
        bool clearInvocations = true)
    {
        FrontendMock.Setup(f => f.RecordingActive).Returns(recordingActive);
        FrontendMock.Setup(f => f.RecordingPaused).Returns(recordingPaused);

        return CreateLabel(() => new RecordingLabel(
            Context, LocaleMock.Object, PubSubMock.Object, TimeProvider,
            LabelMock.Object, WidgetMock.Object), visible, clearInvocations);
    }

    // -----------------------------------------------------------------------
    // Construction
    // -----------------------------------------------------------------------

    [Test]
    public void Constructor_SetsNameAndInactiveValue()
    {
        //Arrange + Act
        _ = Create(clearInvocations: false);

        //Assert
        LabelMock.Verify(s => s.SetText(NamePtr, "Toolbar.Label.Recording"), Times.Once);
        LabelMock.Verify(s => s.SetText(ValuePtr, "Toolbar.Status.Inactive"), Times.Once);
    }

    // -----------------------------------------------------------------------
    // OnObsFrontendEvent — recording lifecycle
    // -----------------------------------------------------------------------

    [Test]
    public void OnObsFrontendEvent_RecordingStarted_SetsZeroElapsed()
    {
        //Arrange
        var label = Create();

        //Act
        label.OnObsFrontendEvent(new ObsFrontendEvent(obs_frontend_event.OBS_FRONTEND_EVENT_RECORDING_STARTED));

        //Assert
        LabelMock.Verify(s => s.SetText(ValuePtr, "00:00:00"), Times.Once);
    }

    [Test]
    public void OnObsFrontendEvent_RecordingPaused_SetsPausedText()
    {
        //Arrange
        var label = Create();
        label.OnObsFrontendEvent(new ObsFrontendEvent(obs_frontend_event.OBS_FRONTEND_EVENT_RECORDING_STARTED));

        //Act
        label.OnObsFrontendEvent(new ObsFrontendEvent(obs_frontend_event.OBS_FRONTEND_EVENT_RECORDING_PAUSED));

        //Assert
        LabelMock.Verify(s => s.SetText(ValuePtr,
            It.Is<string>(v => v.Contains("Toolbar.Status.Paused"))), Times.Once);
    }

    [Test]
    public void OnObsFrontendEvent_RecordingUnpaused_DoesNotUpdateLabel()
    {
        //Arrange — unpaused just restarts the timer; no label update until next heartbeat
        var label = Create();
        label.OnObsFrontendEvent(new ObsFrontendEvent(obs_frontend_event.OBS_FRONTEND_EVENT_RECORDING_STARTED));
        label.OnObsFrontendEvent(new ObsFrontendEvent(obs_frontend_event.OBS_FRONTEND_EVENT_RECORDING_PAUSED));
        LabelMock.Invocations.Clear();

        //Act
        label.OnObsFrontendEvent(new ObsFrontendEvent(obs_frontend_event.OBS_FRONTEND_EVENT_RECORDING_UNPAUSED));

        //Assert
        LabelMock.Verify(s => s.SetText(ValuePtr, It.IsAny<string>()), Times.Never);
    }

    [Test]
    public void OnObsFrontendEvent_RecordingStopped_SetsInactive()
    {
        //Arrange
        var label = Create();
        label.OnObsFrontendEvent(new ObsFrontendEvent(obs_frontend_event.OBS_FRONTEND_EVENT_RECORDING_STARTED));

        //Act
        label.OnObsFrontendEvent(new ObsFrontendEvent(obs_frontend_event.OBS_FRONTEND_EVENT_RECORDING_STOPPED));

        //Assert
        LabelMock.Verify(s => s.SetText(ValuePtr, "Toolbar.Status.Inactive"), Times.AtLeastOnce);
    }

    // -----------------------------------------------------------------------
    // Pause duration accumulation
    // -----------------------------------------------------------------------

    [Test]
    public void PausedDuration_AccumulatesAcrossMultiplePauseCycles()
    {
        //Arrange
        var label = Create();
        label.OnObsFrontendEvent(new ObsFrontendEvent(obs_frontend_event.OBS_FRONTEND_EVENT_RECORDING_STARTED));

        //Act — two 500ms active runs separated by a 200ms pause
        TimeProvider.Advance(TimeSpan.FromMilliseconds(500));
        label.OnObsFrontendEvent(new ObsFrontendEvent(obs_frontend_event.OBS_FRONTEND_EVENT_RECORDING_PAUSED));

        TimeProvider.Advance(TimeSpan.FromMilliseconds(200)); // paused — not counted
        label.OnObsFrontendEvent(new ObsFrontendEvent(obs_frontend_event.OBS_FRONTEND_EVENT_RECORDING_UNPAUSED));

        TimeProvider.Advance(TimeSpan.FromMilliseconds(500));
        label.OnObsFrontendEvent(new ObsFrontendEvent(obs_frontend_event.OBS_FRONTEND_EVENT_RECORDING_PAUSED));

        //Assert — paused text should reflect ~1s accumulated (two 500ms runs), not ~1.2s
        LabelMock.Verify(s => s.SetText(ValuePtr,
            It.Is<string>(v => v.Contains("Toolbar.Status.Paused"))), Times.Exactly(2));
    }

    [Test]
    public void OnObsFrontendEvent_RecordingStopped_ResetsAccumulatedDuration()
    {
        //Arrange
        var label = Create();
        label.OnObsFrontendEvent(new ObsFrontendEvent(obs_frontend_event.OBS_FRONTEND_EVENT_RECORDING_STARTED));
        label.OnObsFrontendEvent(new ObsFrontendEvent(obs_frontend_event.OBS_FRONTEND_EVENT_RECORDING_STOPPED));

        //Act — restart
        label.OnObsFrontendEvent(new ObsFrontendEvent(obs_frontend_event.OBS_FRONTEND_EVENT_RECORDING_STARTED));

        //Assert — 00:00:00 should have been set exactly twice (once per start)
        LabelMock.Verify(s => s.SetText(ValuePtr, "00:00:00"), Times.Exactly(2));
    }

    // -----------------------------------------------------------------------
    // OnHeartbeatEvent
    // -----------------------------------------------------------------------

    [Test]
    public void OnHeartbeatEvent_WhenRecording_UpdatesElapsed()
    {
        //Arrange
        var label = Create();
        label.OnObsFrontendEvent(new ObsFrontendEvent(obs_frontend_event.OBS_FRONTEND_EVENT_RECORDING_STARTED));
        LabelMock.Invocations.Clear();

        //Act
        TimeProvider.Advance(TimeSpan.FromMilliseconds(1100));
        label.OnHeartbeatEvent(new HeartbeatEvent());

        //Assert
        LabelMock.Verify(s => s.SetText(ValuePtr, It.Is<string>(v => v.StartsWith("00:00:"))), Times.Once);
    }

    [Test]
    public void OnHeartbeatEvent_WhenPaused_DoesNotUpdateLabel()
    {
        //Arrange
        var label = Create();
        label.OnObsFrontendEvent(new ObsFrontendEvent(obs_frontend_event.OBS_FRONTEND_EVENT_RECORDING_STARTED));
        label.OnObsFrontendEvent(new ObsFrontendEvent(obs_frontend_event.OBS_FRONTEND_EVENT_RECORDING_PAUSED));
        LabelMock.Invocations.Clear();

        //Act
        label.OnHeartbeatEvent(new HeartbeatEvent());

        //Assert
        LabelMock.Verify(s => s.SetText(ValuePtr, It.IsAny<string>()), Times.Never);
    }

    [Test]
    public void OnHeartbeatEvent_WhenStopped_DoesNotUpdateLabel()
    {
        //Arrange
        var label = Create();
        label.OnObsFrontendEvent(new ObsFrontendEvent(obs_frontend_event.OBS_FRONTEND_EVENT_RECORDING_STARTED));
        label.OnObsFrontendEvent(new ObsFrontendEvent(obs_frontend_event.OBS_FRONTEND_EVENT_RECORDING_STOPPED));
        LabelMock.Invocations.Clear();

        //Act
        label.OnHeartbeatEvent(new HeartbeatEvent());

        //Assert
        LabelMock.Verify(s => s.SetText(ValuePtr, It.IsAny<string>()), Times.Never);
    }

    // -----------------------------------------------------------------------
    // OnSettingsChangedEvent
    // -----------------------------------------------------------------------

    [Test]
    public void OnSettingsChangedEvent_HideRecordingLabel_HidesGroup()
    {
        //Arrange
        var label = Create();

        //Act
        label.OnSettingsChangedEvent(new SettingsChangedEvent(new PluginSettings { HideRecordingLabel = true }));

        //Assert
        WidgetMock.Verify(s => s.SetVisible(It.IsAny<IntPtr>(), false), Times.AtLeastOnce);
    }

    [Test]
    public void OnSettingsChangedEvent_ShowRecordingLabel_ShowsGroup()
    {
        //Arrange — start hidden so a single settings event is enough to assert show
        var label = Create(visible: false);

        //Act
        label.OnSettingsChangedEvent(new SettingsChangedEvent(new PluginSettings { HideRecordingLabel = false }));

        //Assert
        WidgetMock.Verify(s => s.SetVisible(It.IsAny<IntPtr>(), true), Times.AtLeastOnce);
    }

    // -----------------------------------------------------------------------
    // Dispose
    // -----------------------------------------------------------------------

    [Test]
    public void Dispose_UnsubscribesFromAllEvents()
    {
        //Arrange
        var label = Create();

        //Act
        label.Dispose();

        //Assert
        PubSubMock.Verify(p => p.Unsubscribe(It.IsAny<Action<ObsFrontendEvent>>()), Times.Once);
        PubSubMock.Verify(p => p.Unsubscribe(It.IsAny<Action<HeartbeatEvent>>()), Times.Once);
        PubSubMock.Verify(p => p.Unsubscribe(It.IsAny<Action<SettingsChangedEvent>>()), Times.Once);
    }
}
