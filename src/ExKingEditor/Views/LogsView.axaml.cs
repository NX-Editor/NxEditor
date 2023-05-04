using Avalonia.Controls;
using ExKingEditor.ViewModels;

namespace ExKingEditor.Views;
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