using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Platform.Storage;
using ExKingEditor.Generators;
using ExKingEditor.Models;
using ExKingEditor.Models.Editors;
using System.Diagnostics;

namespace ExKingEditor.Views.Editors;
public partial class SarcView : UserControl
{
    private readonly Control _extension;

    public SarcView()
    {
        InitializeComponent();

        _extension = MenuFactory.Generate(new SarcMenu(this))[0];

        DetachedFromVisualTree += (s, e) => ShellView.ClearOverflowMenu();
        AttachedToVisualTree += (s, e) => {
            ShellView.MainMenu?.Add(new Separator {
                Width = 1,
                Height = 12,
                Margin = new(2, 0)
            });

            ShellView.MainMenu?.Add(_extension);
            ShellView.MenuOverflow = 2;
        };

        DropClient.AddHandler(DragDrop.DragEnterEvent, DragEnterEvent);
        DropClient.AddHandler(DragDrop.DragLeaveEvent, DragLeaveEvent);
        DropClient.AddHandler(DragDrop.DropEvent, DragDropEvent);
    }

    public void DragDropEvent(object? sender, DragEventArgs e)
    {
        e.Handled = true;
    }

    public void DragEnterEvent(object? sender, DragEventArgs e)
    {
        DragDrop.SetAllowDrop(App.Desktop!.DropClient, false);
        e.Handled = true;
    }

    public void DragLeaveEvent(object? sender, DragEventArgs e)
    {
        DragDrop.SetAllowDrop(App.Desktop!.DropClient, true);
    }
}
