using DynamiteAndy.DynamiteToolbar.Models.Events;
using DynamiteAndy.DynamiteToolbar.UI.Dock.Labels;
using Moq;
using ObsInterop;
using DynamiteAndy.DynamiteToolbar.Models.Shim;
using PluginSettings = DynamiteAndy.DynamiteToolbar.Models.PluginSettings;

namespace DynamiteAndy.DynamiteToolbar.Tests.Unit.UI.Dock.Labels;

[TestFixture]
public class DroppedFramesLabelTests : LabelTestBase
{
    protected override PluginDock BuildPluginDock() => new()
    {
        Monitor = new DockFrameMonitor { DroppedFrames = MakeDockFrameMonitorLabel() }
    };

    protected override string ExpectedNameKey => "Toolbar.Label.DroppedFrames";

    protected override void CreateForNameLabelTest() => _ = Create(clearInvocations: false);

    private DroppedFramesLabel Create(bool streamingActive = false, bool visible = true, bool clearInvocations = true)
    {
        FrontendMock.Setup(f => f.StreamingActive).Returns(streamingActive);

        return CreateLabel(() => new DroppedFramesLabel(
            Context, ObsApiMock.Object, FrontendMock.Object,
            LocaleMock.Object, PubSubMock.Object,
            LabelMock.Object, WidgetMock.Object
        ), visible, clearInvocations);
    }

    // -----------------------------------------------------------------------
    // Construction
    // -----------------------------------------------------------------------

    [Test]
    public void Constructor_WhenNotStreaming_SettingsEventKeepsLabelHidden()
    {
        //Arrange — constructor stores _visibilityFromStreaming=false; verify via settings event
        var label = Create(streamingActive: false, visible: false);

        //Act
        label.OnSettingsChangedEvent(new SettingsChangedEvent(new PluginSettings { HideDroppedFramesLabel = false }));

        //Assert — streaming=false gates visibility regardless of setting
        WidgetMock.Verify(s => s.SetVisible(It.IsAny<IntPtr>(), true), Times.Never);
    }

    [Test]
    public void Constructor_WhenStreaming_SettingsEventShowsLabel()
    {
        //Arrange — constructor stores _visibilityFromStreaming=true; verify via settings event
        var label = Create(streamingActive: true, visible: false);

        //Act
        label.OnSettingsChangedEvent(new SettingsChangedEvent(new PluginSettings { HideDroppedFramesLabel = false }));

        //Assert — streaming=true allows visibility when setting also allows
        WidgetMock.Verify(s => s.SetVisible(It.IsAny<IntPtr>(), true), Times.AtLeastOnce);
    }

    // -----------------------------------------------------------------------
    // OnObsFrontendEvent — streaming gates visibility
    // -----------------------------------------------------------------------

    [Test]
    public void OnObsFrontendEvent_StreamingStarting_ShowsLabel()
    {
        //Arrange — start hidden so the Visible guard doesn't block the shim call
        var label = Create(visible: false);

        //Act
        label.OnObsFrontendEvent(new ObsFrontendEvent(obs_frontend_event.OBS_FRONTEND_EVENT_STREAMING_STARTING));

        //Assert
        WidgetMock.Verify(s => s.SetVisible(It.IsAny<IntPtr>(), true), Times.AtLeastOnce);
    }

    [Test]
    public void OnObsFrontendEvent_StreamingStarted_ShowsLabel()
    {
        //Arrange — start hidden so the Visible guard doesn't block the shim call
        var label = Create(visible: false);

        //Act
        label.OnObsFrontendEvent(new ObsFrontendEvent(obs_frontend_event.OBS_FRONTEND_EVENT_STREAMING_STARTED));

        //Assert
        WidgetMock.Verify(s => s.SetVisible(It.IsAny<IntPtr>(), true), Times.AtLeastOnce);
    }

    [Test]
    public void OnObsFrontendEvent_StreamingStopping_ShowsLabel()
    {
        //Arrange — start hidden so the Visible guard doesn't block the shim call
        var label = Create(visible: false);

        //Act
        label.OnObsFrontendEvent(new ObsFrontendEvent(obs_frontend_event.OBS_FRONTEND_EVENT_STREAMING_STOPPING));

        //Assert
        WidgetMock.Verify(s => s.SetVisible(It.IsAny<IntPtr>(), true), Times.AtLeastOnce);
    }

    [Test]
    public void OnObsFrontendEvent_StreamingStopped_HidesLabelAndResetsValue()
    {
        //Arrange
        var label = Create(streamingActive: true);

        //Act
        label.OnObsFrontendEvent(new ObsFrontendEvent(obs_frontend_event.OBS_FRONTEND_EVENT_STREAMING_STOPPED));

        //Assert
        WidgetMock.Verify(s => s.SetVisible(It.IsAny<IntPtr>(), false), Times.AtLeastOnce);
        LabelMock.Verify(s => s.SetText(ValuePtr, "0 (0.0%)"), Times.Once);
    }

    [Test]
    public void OnObsFrontendEvent_UnrelatedEvent_DoesNotChangeVisibility()
    {
        //Arrange
        var label = Create();

        //Act
        label.OnObsFrontendEvent(new ObsFrontendEvent(obs_frontend_event.OBS_FRONTEND_EVENT_RECORDING_STARTED));

        //Assert
        WidgetMock.Verify(s => s.SetVisible(It.IsAny<IntPtr>(), It.IsAny<bool>()), Times.Never);
    }

    // -----------------------------------------------------------------------
    // OnHeartbeatEvent
    // -----------------------------------------------------------------------

    [Test]
    public void OnHeartbeatEvent_WhenNotStreaming_DoesNotQueryOutput()
    {
        //Arrange — not streaming, so _visibilityFromStreaming=false
        var label = Create(streamingActive: false, visible: false);

        //Act
        label.OnHeartbeatEvent(new HeartbeatEvent());

        //Assert
        FrontendMock.Verify(f => f.GetStreamingOutput(), Times.Never);
    }

    [Test]
    public void OnHeartbeatEvent_WhenStreaming_FormatsDroppedFramesAndReleasesOutput()
    {
        //Arrange
        var label = Create(streamingActive: true);
        var dummyOutput = new IntPtr(0x55555);
        FrontendMock.Setup(f => f.GetStreamingOutput()).Returns(dummyOutput);
        ObsApiMock.Setup(a => a.OutputGetFramesDropped(dummyOutput)).Returns(3);
        ObsApiMock.Setup(a => a.OutputGetTotalFrames(dummyOutput)).Returns(100);

        //Act
        label.OnHeartbeatEvent(new HeartbeatEvent());

        //Assert — 3/100 * 100 = 3.0%
        LabelMock.Verify(s => s.SetText(ValuePtr, "3 (3.0%)"), Times.Once);
        ObsApiMock.Verify(a => a.OutputRelease(dummyOutput), Times.Once);
    }

    [Test]
    public void OnHeartbeatEvent_WhenTotalFramesIsZero_ShowsZeroPercent()
    {
        //Arrange — total=0 guards against division by zero; result should be 0.0%
        var label = Create(streamingActive: true);
        var dummyOutput = new IntPtr(0x55555);
        FrontendMock.Setup(f => f.GetStreamingOutput()).Returns(dummyOutput);
        ObsApiMock.Setup(a => a.OutputGetFramesDropped(dummyOutput)).Returns(0);
        ObsApiMock.Setup(a => a.OutputGetTotalFrames(dummyOutput)).Returns(0);

        //Act
        label.OnHeartbeatEvent(new HeartbeatEvent());

        //Assert
        LabelMock.Verify(s => s.SetText(ValuePtr, "0 (0.0%)"), Times.Once);
        ObsApiMock.Verify(a => a.OutputRelease(dummyOutput), Times.Once);
    }

    [Test]
    public void OnHeartbeatEvent_WhenOutputHandleIsZero_DoesNotUpdateLabel()
    {
        //Arrange
        var label = Create(streamingActive: true);
        FrontendMock.Setup(f => f.GetStreamingOutput()).Returns(IntPtr.Zero);

        //Act
        label.OnHeartbeatEvent(new HeartbeatEvent());

        //Assert
        LabelMock.Verify(s => s.SetText(ValuePtr, It.IsAny<string>()), Times.Never);
        ObsApiMock.Verify(a => a.OutputRelease(It.IsAny<IntPtr>()), Times.Never);
    }

    // -----------------------------------------------------------------------
    // OnSettingsChangedEvent — settings gate visibility
    // -----------------------------------------------------------------------

    [Test]
    public void OnSettingsChangedEvent_HideDroppedFrames_HidesLabel()
    {
        //Arrange
        var label = Create(streamingActive: true);

        //Act
        label.OnSettingsChangedEvent(new SettingsChangedEvent(new PluginSettings { HideDroppedFramesLabel = true }));

        //Assert
        WidgetMock.Verify(s => s.SetVisible(It.IsAny<IntPtr>(), false), Times.AtLeastOnce);
    }

    [Test]
    public void OnSettingsChangedEvent_ShowDroppedFrames_WhileStreaming_ShowsLabel()
    {
        //Arrange — start hidden while streaming so a single settings event is enough to assert show
        var label = Create(streamingActive: true, visible: false);

        //Act
        label.OnSettingsChangedEvent(new SettingsChangedEvent(new PluginSettings { HideDroppedFramesLabel = false }));

        //Assert
        WidgetMock.Verify(s => s.SetVisible(It.IsAny<IntPtr>(), true), Times.AtLeastOnce);
    }

    [Test]
    public void OnSettingsChangedEvent_ShowDroppedFrames_WhileNotStreaming_RemainsHidden()
    {
        //Arrange — start hidden while not streaming so a single settings event is enough to assert hidden
        var label = Create(streamingActive: false, visible: false);

        //Act
        label.OnSettingsChangedEvent(new SettingsChangedEvent(new PluginSettings { HideDroppedFramesLabel = false }));

        //Assert
        WidgetMock.Verify(s => s.SetVisible(It.IsAny<IntPtr>(), true), Times.Never);
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
