using Avalonia.Media.Imaging;
using Avalonia.Platform;
using FluentAvalonia.UI.Windowing;

namespace NxEditor.Views;

public partial class ShellView : AppWindow
{
    public ShellView()
    {
        InitializeComponent();

        TitleBar.TitleBarHitTestType = TitleBarHitTestType.Complex;
        TitleBar.ExtendsContentIntoTitleBar = true;

        const string AVARES_ICON_PATH = "avares://NxEditor/Assets/Icon.ico";
        Bitmap bitmap = new(AssetLoader.Open(new Uri(AVARES_ICON_PATH)));
        Icon = bitmap.CreateScaledBitmap(new(48, 48), BitmapInterpolationMode.HighQuality);
        IconHost.Source = Icon;
    }
}
