using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using ExKingEditor.ViewModels;
using ExKingEditor.Views;

namespace ExKingEditor;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop) {
            desktop.MainWindow = new ShellView() {
                DataContext = ShellViewModel.Shared
            };
        }

        base.OnFrameworkInitializationCompleted();
    }
}
