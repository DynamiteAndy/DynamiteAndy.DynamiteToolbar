using DynamiteAndy.DynamiteToolbar.Models.Events;
using DynamiteAndy.DynamiteToolbar.Models.Shim;
using DynamiteAndy.DynamiteToolbar.UI.Dock.Labels;
using Moq;
using ObsInterop;
using PluginSettings = DynamiteAndy.DynamiteToolbar.Models.PluginSettings;

namespace DynamiteAndy.DynamiteToolbar.Tests.Unit.UI.Dock.Labels;

[TestFixture]
public class StreamingLabelTests : LabelTestBase
{
    protected override PluginDock BuildPluginDock() => new()
    {
        Monitor = new DockFrameMonitor { Streaming = MakeDockFrameMonitorLabel() }
    };

    protected override string ExpectedNameKey => "Toolbar.Label.Streaming";

    protected override void CreateForNameLabelTest() => _ = Create(clearInvocations: false);

    private StreamingLabel Create(
        bool streamingActive = false,
        bool visible = true,
        bool clearInvocations = true)
    {
        FrontendMock.Setup(f => f.StreamingActive).Returns(streamingActive);

        return CreateLabel(() => new StreamingLabel(
            Context, LocaleMock.Object, PubSubMock.Object, TimeProvider,
            LabelMock.Object, WidgetMock.Object), visible, clearInvocations);
    }

    // -----------------------------------------------------------------------
    // Construction
    // -----------------------------------------------------------------------

    [Test]
    public void Constructor_SetsNameAndInitialInactiveValue()
    {
        //Arrange + Act
        _ = Create(clearInvocations: false);

        //Assert
        LabelMock.Verify(s => s.SetText(NamePtr, "Toolbar.Label.Streaming"), Times.Once);
        LabelMock.Verify(s => s.SetText(ValuePtr, "Toolbar.Status.Inactive"), Times.Once);
    }

    [Test]
    public void Constructor_SubscribesToThreeEvents()
    {
        //Arrange + Act
        _ = Create(clearInvocations: false);

        //Assert
        PubSubMock.Verify(p => p.Subscribe(It.IsAny<Action<ObsFrontendEvent>>()), Times.Once);
        PubSubMock.Verify(p => p.Subscribe(It.IsAny<Action<HeartbeatEvent>>()), Times.Once);
        PubSubMock.Verify(p => p.Subscribe(It.IsAny<Action<SettingsChangedEvent>>()), Times.Once);
    }

    // -----------------------------------------------------------------------
    // OnObsFrontendEvent
    // -----------------------------------------------------------------------

    [Test]
    public void OnObsFrontendEvent_Starting_SetsStartingText()
    {
        //Arrange
        var label = Create();

        //Act
        label.OnObsFrontendEvent(new ObsFrontendEvent(obs_frontend_event.OBS_FRONTEND_EVENT_STREAMING_STARTING));

        //Assert
        LabelMock.Verify(s => s.SetText(ValuePtr, "Toolbar.Status.Starting"), Times.Once);
    }

    [Test]
    public void OnObsFrontendEvent_Started_SetsZeroElapsed()
    {
        //Arrange
        var label = Create();

        //Act
        label.OnObsFrontendEvent(new ObsFrontendEvent(obs_frontend_event.OBS_FRONTEND_EVENT_STREAMING_STARTED));

        //Assert
        LabelMock.Verify(s => s.SetText(ValuePtr, "00:00:00"), Times.Once);
    }

    [Test]
    public void OnObsFrontendEvent_Stopping_SetsStoppingText()
    {
        //Arrange
        var label = Create();

        //Act
        label.OnObsFrontendEvent(new ObsFrontendEvent(obs_frontend_event.OBS_FRONTEND_EVENT_STREAMING_STOPPING));

        //Assert
        LabelMock.Verify(s => s.SetText(ValuePtr, "Toolbar.Status.Stopping"), Times.Once);
    }

    [Test]
    public void OnObsFrontendEvent_Stopped_SetsInactiveAndClearsTimer()
    {
        //Arrange
        var label = Create();
        label.OnObsFrontendEvent(new ObsFrontendEvent(obs_frontend_event.OBS_FRONTEND_EVENT_STREAMING_STARTED));

        //Act
        label.OnObsFrontendEvent(new ObsFrontendEvent(obs_frontend_event.OBS_FRONTEND_EVENT_STREAMING_STOPPED));

        //Assert
        LabelMock.Verify(s => s.SetText(ValuePtr, "Toolbar.Status.Inactive"), Times.AtLeastOnce);
    }

    [Test]
    public void OnObsFrontendEvent_UnrelatedEvent_DoesNotChangeValue()
    {
        //Arrange
        var label = Create();

        //Act
        label.OnObsFrontendEvent(new ObsFrontendEvent(obs_frontend_event.OBS_FRONTEND_EVENT_RECORDING_STARTED));

        //Assert
        LabelMock.Verify(s => s.SetText(ValuePtr, It.IsAny<string>()), Times.Never);
    }

    // -----------------------------------------------------------------------
    // OnHeartbeatEvent
    // -----------------------------------------------------------------------

    [Test]
    public void OnHeartbeatEvent_BeforeStarted_DoesNotUpdateValue()
    {
        //Arrange
        var label = Create();

        //Act
        label.OnHeartbeatEvent(new HeartbeatEvent());

        //Assert
        LabelMock.Verify(s => s.SetText(ValuePtr, It.IsAny<string>()), Times.Never);
    }

    [Test]
    public void OnHeartbeatEvent_AfterStarted_UpdatesElapsed()
    {
        //Arrange
        var label = Create();
        label.OnObsFrontendEvent(new ObsFrontendEvent(obs_frontend_event.OBS_FRONTEND_EVENT_STREAMING_STARTED));
        LabelMock.Invocations.Clear();

        //Act
        TimeProvider.Advance(TimeSpan.FromMilliseconds(1100));
        label.OnHeartbeatEvent(new HeartbeatEvent());

        //Assert
        LabelMock.Verify(s => s.SetText(ValuePtr, It.Is<string>(v => v.StartsWith("00:00:"))), Times.Once);
    }

    [Test]
    public void OnHeartbeatEvent_AfterStopped_DoesNotUpdateValue()
    {
        //Arrange
        var label = Create();
        label.OnObsFrontendEvent(new ObsFrontendEvent(obs_frontend_event.OBS_FRONTEND_EVENT_STREAMING_STARTED));
        label.OnObsFrontendEvent(new ObsFrontendEvent(obs_frontend_event.OBS_FRONTEND_EVENT_STREAMING_STOPPED));
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
    public void OnSettingsChangedEvent_HideStreamingLabel_HidesGroup()
    {
        //Arrange
        var label = Create();

        //Act
        label.OnSettingsChangedEvent(new SettingsChangedEvent(new PluginSettings { HideStreamingLabel = true }));

        //Assert
        WidgetMock.Verify(s => s.SetVisible(It.IsAny<IntPtr>(), false), Times.AtLeastOnce);
    }

    [Test]
    public void OnSettingsChangedEvent_ShowStreamingLabel_ShowsGroup()
    {
        //Arrange — start hidden so a single settings event is enough to assert show
        var label = Create(visible: false);

        //Act
        label.OnSettingsChangedEvent(new SettingsChangedEvent(new PluginSettings { HideStreamingLabel = false }));

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
