using DynamiteAndy.DynamiteToolbar.Models.Events;
using DynamiteAndy.DynamiteToolbar.Models.Shim;
using DynamiteAndy.DynamiteToolbar.UI.Dock.Labels;
using Moq;
using ObsInterop;
using PluginSettings = DynamiteAndy.DynamiteToolbar.Models.PluginSettings;

namespace DynamiteAndy.DynamiteToolbar.Tests.Unit.UI.Dock.Labels;

[TestFixture]
public class FpsLabelTests : LabelTestBase
{
    public delegate void GetVideoInfoCallback(ref obs_video_info ovi);

    protected override PluginDock BuildPluginDock() => new()
    {
        Monitor = new DockFrameMonitor { Fps = MakeDockFrameMonitorLabel() }
    };

    protected override string ExpectedNameKey => "Toolbar.Label.FPS";

    protected override void CreateForNameLabelTest() => _ = Create(clearInvocations: false);

    private FpsLabel Create(bool visible = true, bool clearInvocations = true) => CreateLabel(() => new FpsLabel(
        Context, ObsApiMock.Object, LocaleMock.Object, PubSubMock.Object,
        LabelMock.Object, WidgetMock.Object), visible, clearInvocations);

    // -----------------------------------------------------------------------
    // Construction
    // -----------------------------------------------------------------------

    [Test]
    public void Constructor_SubscribesToExpectedEvents()
    {
        //Arrange + Act
        _ = Create();

        //Assert
        PubSubMock.Verify(p => p.Subscribe(It.IsAny<Action<HeartbeatEvent>>()), Times.Once);
        PubSubMock.Verify(p => p.Subscribe(It.IsAny<Action<SettingsChangedEvent>>()), Times.Once);
    }

    // -----------------------------------------------------------------------
    // OnHeartbeatEvent
    // -----------------------------------------------------------------------

    [Test]
    public void OnHeartbeatEvent_WhenGetVideoInfoSucceeds_CalculatesTargetFromStruct()
    {
        //Arrange
        var label = Create();
        ObsApiMock.Setup(a => a.GetVideoInfo(ref It.Ref<obs_video_info>.IsAny))
            .Callback(new GetVideoInfoCallback((ref ovi) =>
            {
                ovi.fps_num = 60000;
                ovi.fps_den = 1001; // Common NTSC 59.94 FPS configuration
            }))
            .Returns(1); // Non-zero indicates success in OBS
        ObsApiMock.Setup(a => a.GetActiveFps()).Returns(58.324);

        //Act
        label.OnHeartbeatEvent(new HeartbeatEvent());

        //Assert — 60000 / 1001 = 59.94005... -> formats to 59.94
        LabelMock.Verify(s => s.SetText(ValuePtr, "58.32 / 59.94"), Times.Once);
    }

    [Test]
    public void OnHeartbeatEvent_WhenGetVideoInfoFails_DefaultsTargetToSixty()
    {
        //Arrange
        var label = Create();
        ObsApiMock.Setup(a => a.GetVideoInfo(ref It.Ref<obs_video_info>.IsAny)).Returns(0); // 0 = OBS system failure
        ObsApiMock.Setup(a => a.GetActiveFps()).Returns(60.0);

        //Act
        label.OnHeartbeatEvent(new HeartbeatEvent());

        //Assert — target falls back precisely to 60.00
        LabelMock.Verify(s => s.SetText(ValuePtr, "60.00 / 60.00"), Times.Once);
    }

    // -----------------------------------------------------------------------
    // OnSettingsChangedEvent — settings gate visibility
    // -----------------------------------------------------------------------

    [Test]
    public void OnSettingsChangedEvent_HideFpsLabel_HidesLabel()
    {
        //Arrange
        var label = Create();

        //Act
        label.OnSettingsChangedEvent(new SettingsChangedEvent(new PluginSettings { HideFpsLabel = true }));

        //Assert
        WidgetMock.Verify(s => s.SetVisible(It.IsAny<IntPtr>(), false), Times.AtLeastOnce);
    }

    [Test]
    public void OnSettingsChangedEvent_ShowFpsLabel_ShowsLabel()
    {
        //Arrange — start hidden so a single settings event is enough to assert show
        var label = Create(visible: false);

        //Act
        label.OnSettingsChangedEvent(new SettingsChangedEvent(new PluginSettings { HideFpsLabel = false }));

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
        PubSubMock.Verify(p => p.Unsubscribe(It.IsAny<Action<HeartbeatEvent>>()), Times.Once);
        PubSubMock.Verify(p => p.Unsubscribe(It.IsAny<Action<SettingsChangedEvent>>()), Times.Once);
    }
}
