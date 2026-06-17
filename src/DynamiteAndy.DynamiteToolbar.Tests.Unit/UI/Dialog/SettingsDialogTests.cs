using DynamiteAndy.DynamiteToolbar.Interop.Bridge;
using DynamiteAndy.DynamiteToolbar.Interop.Bridge.Qt;
using DynamiteAndy.DynamiteToolbar.Models.Events;
using DynamiteAndy.DynamiteToolbar.Models.Shim;
using DynamiteAndy.DynamiteToolbar.Services;
using DynamiteAndy.DynamiteToolbar.Services.Settings;
using DynamiteAndy.DynamiteToolbar.UI.Dialog;
using Moq;
using PluginSettings = DynamiteAndy.DynamiteToolbar.Models.PluginSettings;
using ShimPluginSettings = DynamiteAndy.DynamiteToolbar.Models.Shim.PluginSettings;

namespace DynamiteAndy.DynamiteToolbar.Tests.Unit.UI.Dialog;

[TestFixture]
public class SettingsDialogTests
{
    private Mock<ISettingsDialogShim> _dialogShimMock = null!;
    private Mock<ISettingsService> _settingsMock = null!;
    private Mock<ILocalizationService> _localeMock = null!;
    private Mock<IQGroupBoxShim> _groupBoxMock = null!;
    private Mock<IQAbstractButtonShim> _btnMock = null!;
    private Mock<IQLabelShim> _labelMock = null!;
    private Mock<IPubSubService> _pubSubMock = null!;

    private static readonly IntPtr DialogPtr = new(0xDEAD);

    private PluginSettingsDialog _dialogStub = new()
    {
        PluginSettingsDialogPtr = new IntPtr(0xDEAD),
        GroupBoxes = new SettingsDialogGroupBoxes(),
        Checkboxes = new SettingsDialogCheckboxes(),
        Labels = new SettingsDialogLabels(),
    };

    [SetUp]
    public void SetUp()
    {
        _dialogShimMock = new Mock<ISettingsDialogShim>();
        _settingsMock = new Mock<ISettingsService>();
        _localeMock = new Mock<ILocalizationService>();
        _groupBoxMock = new Mock<IQGroupBoxShim>();
        _btnMock = new Mock<IQAbstractButtonShim>();
        _labelMock = new Mock<IQLabelShim>();
        _pubSubMock = new Mock<IPubSubService>();

        _localeMock.Setup(l => l.T(It.IsAny<string>())).Returns<string>(k => k);
        _settingsMock.Setup(s => s.Load()).Returns(new PluginSettings());
        _dialogShimMock.Setup(d => d.CreateSettingsDialog(It.IsAny<ShimPluginSettings>()))
                       .Returns(_dialogStub);
    }

    private SettingsDialog CreateSut() => new(
        _dialogShimMock.Object, _settingsMock.Object, _localeMock.Object,
        _groupBoxMock.Object, _btnMock.Object, _labelMock.Object, _pubSubMock.Object);

    [Test]
    public void Show_WhenCreateReturnsZeroPtr_DoesNotCallShow()
    {
        _dialogShimMock.Setup(d => d.CreateSettingsDialog(It.IsAny<ShimPluginSettings>()))
                       .Returns(new PluginSettingsDialog { PluginSettingsDialogPtr = IntPtr.Zero });

        CreateSut().Show();

        _dialogShimMock.Verify(
            d => d.ShowSettingsDialog(It.IsAny<IntPtr>(), ref It.Ref<ShimPluginSettings>.IsAny),
            Times.Never);
    }

    [Test]
    public void Show_SetsGroupBoxTitles()
    {
        _dialogShimMock.Setup(d => d.ShowSettingsDialog(
            It.IsAny<IntPtr>(), ref It.Ref<ShimPluginSettings>.IsAny)).Returns(0);

        CreateSut().Show();

        _groupBoxMock.Verify(g => g.SetTitle(It.IsAny<IntPtr>(),
            "Toolbar.Settings.ShowHideButtons.Title"), Times.Once);
        _groupBoxMock.Verify(g => g.SetTitle(It.IsAny<IntPtr>(),
            "Toolbar.Settings.ShowHideLabels.Title"), Times.Once);
        _groupBoxMock.Verify(g => g.SetTitle(It.IsAny<IntPtr>(),
            "Toolbar.Settings.ShowHideConfirmationModals.Title"), Times.Once);
    }

    [Test]
    public void Show_SetsAllCheckboxTexts()
    {
        _dialogShimMock.Setup(d => d.ShowSettingsDialog(
            It.IsAny<IntPtr>(), ref It.Ref<ShimPluginSettings>.IsAny)).Returns(0);

        CreateSut().Show();

        _btnMock.Verify(b => b.SetText(It.IsAny<IntPtr>(), "Toolbar.Button.Float.Setting"), Times.Once);
        _btnMock.Verify(b => b.SetText(It.IsAny<IntPtr>(), "Toolbar.Button.Stream.Setting"), Times.Once);
        _btnMock.Verify(b => b.SetText(It.IsAny<IntPtr>(), "Toolbar.Label.DroppedFrames.Setting"), Times.Once);
        _btnMock.Verify(b => b.SetText(It.IsAny<IntPtr>(), "ConfirmationModal.Recording.Setting"), Times.Once);
    }

    [Test]
    public void Show_SetsCreditsAndContributorsLabels()
    {
        _dialogShimMock.Setup(d => d.ShowSettingsDialog(
            It.IsAny<IntPtr>(), ref It.Ref<ShimPluginSettings>.IsAny)).Returns(0);

        CreateSut().Show();

        _labelMock.Verify(l => l.SetText(It.IsAny<IntPtr>(), "Toolbar.Settings.Credits"), Times.Once);
        _labelMock.Verify(l => l.SetText(It.IsAny<IntPtr>(), "Toolbar.Settings.Contributors"), Times.Once);
    }

    [Test]
    public void Show_WhenAccepted_SavesSettings()
    {
        _dialogShimMock.Setup(d => d.ShowSettingsDialog(
            It.IsAny<IntPtr>(), ref It.Ref<ShimPluginSettings>.IsAny)).Returns(1);

        CreateSut().Show();

        _settingsMock.Verify(s => s.Save(It.IsAny<PluginSettings>()), Times.Once);
    }

    [Test]
    public void Show_WhenAccepted_RaisesSettingsChangedEvent()
    {
        _dialogShimMock.Setup(d => d.ShowSettingsDialog(
            It.IsAny<IntPtr>(), ref It.Ref<ShimPluginSettings>.IsAny)).Returns(1);

        CreateSut().Show();

        _pubSubMock.Verify(p => p.Raise(It.IsAny<SettingsChangedEvent>()), Times.Once);
    }

    [Test]
    public void Show_WhenCancelled_DoesNotSave()
    {
        _dialogShimMock.Setup(d => d.ShowSettingsDialog(
            It.IsAny<IntPtr>(), ref It.Ref<ShimPluginSettings>.IsAny)).Returns(0);

        CreateSut().Show();

        _settingsMock.Verify(s => s.Save(It.IsAny<PluginSettings>()), Times.Never);
    }

    [Test]
    public void Show_WhenCancelled_DoesNotRaiseEvent()
    {
        _dialogShimMock.Setup(d => d.ShowSettingsDialog(
            It.IsAny<IntPtr>(), ref It.Ref<ShimPluginSettings>.IsAny)).Returns(0);

        CreateSut().Show();

        _pubSubMock.Verify(p => p.Raise(It.IsAny<SettingsChangedEvent>()), Times.Never);
    }

    [Test]
    public void Show_AlwaysDestroysDialog_OnAccept()
    {
        _dialogShimMock.Setup(d => d.ShowSettingsDialog(
            It.IsAny<IntPtr>(), ref It.Ref<ShimPluginSettings>.IsAny)).Returns(1);

        CreateSut().Show();

        _dialogShimMock.Verify(d => d.DestroySettingsDialog(DialogPtr), Times.Once);
    }

    [Test]
    public void Show_AlwaysDestroysDialog_OnCancel()
    {
        _dialogShimMock.Setup(d => d.ShowSettingsDialog(
            It.IsAny<IntPtr>(), ref It.Ref<ShimPluginSettings>.IsAny)).Returns(0);

        CreateSut().Show();

        _dialogShimMock.Verify(d => d.DestroySettingsDialog(DialogPtr), Times.Once);
    }

    [Test]
    public void Show_AlwaysDestroysDialog_WhenShowThrows()
    {
        _dialogShimMock.Setup(d => d.ShowSettingsDialog(
            It.IsAny<IntPtr>(), ref It.Ref<ShimPluginSettings>.IsAny))
            .Throws<InvalidOperationException>();

        Assert.Throws<InvalidOperationException>(() => CreateSut().Show());

        _dialogShimMock.Verify(d => d.DestroySettingsDialog(DialogPtr), Times.Once);
    }
}
