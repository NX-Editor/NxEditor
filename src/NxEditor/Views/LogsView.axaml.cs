using Avalonia.Controls;
using NxEditor.ViewModels;

namespace NxEditor.Views;
public partial class LogsView : UserControl
{
    public LogsView()
    {
        InitializeComponent();
        LogsClient.ScrollIntoView(LogsClient.ItemCount - 1);
    }

    protected override void OnDataContextChanged(EventArgs e)
    {
        (DataContext as LogsViewModel)?.InjectView(this);
        base.OnDataContextChanged(e);
    }
}