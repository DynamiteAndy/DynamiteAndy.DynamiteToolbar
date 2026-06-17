using DynamiteAndy.DynamiteToolbar.Models.Events;
using DynamiteAndy.DynamiteToolbar.Models.Shim;
using DynamiteAndy.DynamiteToolbar.UI.Dock.Labels;
using Moq;
using ObsInterop;
using PluginSettings = DynamiteAndy.DynamiteToolbar.Models.PluginSettings;

namespace DynamiteAndy.DynamiteToolbar.Tests.Unit.UI.Dock.Labels;

[TestFixture]
public class CurrentSceneLabelTests : LabelTestBase
{
    private static readonly IntPtr DummySceneHandle = new(0xABC123);

    protected override PluginDock BuildPluginDock() => new()
    {
        Monitor = new DockFrameMonitor { CurrentScene = MakeDockFrameMonitorLabel() }
    };

    protected override string ExpectedNameKey => "Toolbar.Label.CurrentScene";

    protected override void CreateForNameLabelTest() => _ = Create(clearInvocations: false);

    private CurrentSceneLabel Create(bool visible = true, bool clearInvocations = true) => CreateLabel(() => new CurrentSceneLabel(
        Context, ObsApiMock.Object, FrontendMock.Object,
        LocaleMock.Object, PubSubMock.Object,
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
        PubSubMock.Verify(p => p.Subscribe(It.IsAny<Action<ObsFrontendEvent>>()), Times.Once);
        PubSubMock.Verify(p => p.Subscribe(It.IsAny<Action<SettingsChangedEvent>>()), Times.Once);
    }

    // -----------------------------------------------------------------------
    // OnObsFrontendEvent
    // -----------------------------------------------------------------------

    [Test]
    public void OnObsFrontendEvent_SceneChanged_UpdatesLabelAndReleasesSource()
    {
        //Arrange
        var label = Create();
        FrontendMock.Setup(f => f.GetCurrentScene()).Returns(DummySceneHandle);
        ObsApiMock.Setup(a => a.SourceGetName(DummySceneHandle)).Returns("Gaming Scene");

        //Act
        label.OnObsFrontendEvent(new ObsFrontendEvent(obs_frontend_event.OBS_FRONTEND_EVENT_SCENE_CHANGED));

        //Assert
        LabelMock.Verify(s => s.SetText(ValuePtr, "Gaming Scene"), Times.Once);
        ObsApiMock.Verify(a => a.SourceRelease(DummySceneHandle), Times.Once);
    }

    [Test]
    public void OnObsFrontendEvent_SceneChanged_WhenNameIsNull_FallsBackToDash()
    {
        //Arrange
        var label = Create();
        FrontendMock.Setup(f => f.GetCurrentScene()).Returns(DummySceneHandle);
        ObsApiMock.Setup(a => a.SourceGetName(DummySceneHandle)).Returns((string?)null);

        //Act
        label.OnObsFrontendEvent(new ObsFrontendEvent(obs_frontend_event.OBS_FRONTEND_EVENT_SCENE_CHANGED));

        //Assert
        LabelMock.Verify(s => s.SetText(ValuePtr, "-"), Times.Once);
        ObsApiMock.Verify(a => a.SourceRelease(DummySceneHandle), Times.Once);
    }

    [Test]
    public void OnObsFrontendEvent_SceneChanged_WhenHandleIsZero_SetsFallbackTextWithoutRelease()
    {
        //Arrange
        var label = Create();
        FrontendMock.Setup(f => f.GetCurrentScene()).Returns(IntPtr.Zero);

        //Act
        label.OnObsFrontendEvent(new ObsFrontendEvent(obs_frontend_event.OBS_FRONTEND_EVENT_SCENE_CHANGED));

        //Assert
        LabelMock.Verify(s => s.SetText(ValuePtr, "-"), Times.Once);
        ObsApiMock.Verify(a => a.SourceGetName(It.IsAny<IntPtr>()), Times.Never);
        ObsApiMock.Verify(a => a.SourceRelease(It.IsAny<IntPtr>()), Times.Never);
    }

    [Test]
    public void OnObsFrontendEvent_UnrelatedEvent_DoesNotQuerySceneOrUpdateText()
    {
        //Arrange
        var label = Create();

        //Act
        label.OnObsFrontendEvent(new ObsFrontendEvent(obs_frontend_event.OBS_FRONTEND_EVENT_STREAMING_STARTED));

        //Assert
        FrontendMock.Verify(f => f.GetCurrentScene(), Times.Never);
        LabelMock.Verify(s => s.SetText(ValuePtr, It.IsAny<string>()), Times.Never);
    }

    // -----------------------------------------------------------------------
    // OnSettingsChangedEvent
    // -----------------------------------------------------------------------

    [Test]
    public void OnSettingsChangedEvent_HideCurrentSceneLabel_HidesLabel()
    {
        //Arrange
        var label = Create();

        //Act
        label.OnSettingsChangedEvent(new SettingsChangedEvent(new PluginSettings { HideCurrentSceneLabel = true }));

        //Assert
        WidgetMock.Verify(s => s.SetVisible(It.IsAny<IntPtr>(), false), Times.AtLeastOnce);
    }

    [Test]
    public void OnSettingsChangedEvent_ShowCurrentSceneLabel_ShowsLabel()
    {
        //Arrange — start hidden so a single settings event is enough to assert show
        var label = Create(visible: false);

        //Act
        label.OnSettingsChangedEvent(new SettingsChangedEvent(new PluginSettings { HideCurrentSceneLabel = false }));

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
        PubSubMock.Verify(p => p.Unsubscribe(It.IsAny<Action<SettingsChangedEvent>>()), Times.Once);
    }
}
