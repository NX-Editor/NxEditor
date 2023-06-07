using Avalonia.Interactivity;
using CommunityToolkit.Mvvm.ComponentModel;
using NxEditor.Helpers;
using NxEditor.Views.Dialogs;

namespace NxEditor.Dialogs;

public enum ContentDialogResult
{
    None, Primary, Secondary
}

public enum ContentDialogDefaultButton
{
    None, Primary, Secondary
}

public partial class ContentDialog : ObservableObject
{
    private bool _isDialogWaiting = true;
    private ContentDialogResult _result = ContentDialogResult.Primary;
    private readonly ContentDialogView _dialog;

    public ContentDialog()
    {
        _dialog = new(this);
    }

    [ObservableProperty]
    private object? _content;

    [ObservableProperty]
    private string? _title;

    [ObservableProperty]
    private object _primaryButtonContent = "Ok";

    [ObservableProperty]
    private bool _isPrimaryButtonVisible = true;

    [ObservableProperty]
    private Func<RoutedEventArgs, Task>? _primaryButtonAction;

    [ObservableProperty]
    private object _secondaryButtonContent = "Cancel";

    [ObservableProperty]
    private bool _isSecondaryButtonVisible = true;

    [ObservableProperty]
    private Func<RoutedEventArgs, Task>? _secondaryButtonAction;

    public async Task PrimaryButtonCommand()
    {
        RoutedEventArgs args = new();
        if (SecondaryButtonAction?.Invoke(args) is Task task) {
            await task;
        }

        _result = ContentDialogResult.Primary;
        if (!args.Handled) {
            Close();
        }
    }

    public async Task SecondaryButtonCommand()
    {
        RoutedEventArgs args = new();
        if (SecondaryButtonAction?.Invoke(args) is Task task) {
            await task;
        }

        _result = ContentDialogResult.Secondary;
        if (!args.Handled) {
            Close();
        }
    }

    public void Close()
    {
        _dialog.Hide();
        _isDialogWaiting = false;
    }


    public async Task<ContentDialogResult> ShowAsync()
    {
        _dialog.Show();
        await Task.Run(() => {
            while (_isDialogWaiting) { }
        });

        return _result;
    }
}
