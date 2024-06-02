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

        Bitmap bitmap = new(AssetLoader.Open(new Uri("avares://NxEditor/Assets/Icon.ico")));
        Icon = bitmap.CreateScaledBitmap(new(48, 48), BitmapInterpolationMode.HighQuality);
    }
}
