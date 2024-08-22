using Avalonia;
using Avalonia.Controls;
using FluentAvalonia.UI.Controls;

namespace NxEditor;

public class NxeFrontend(Visual xamlRoot) : INxeFrontend
{
    public Visual XamlRoot { get; } = xamlRoot;

    public async ValueTask<T?> PickOneAsync<T>(IAsyncEnumerable<T> values)
    {
        T[] valueArray = await values.ToArrayAsync();
        
        if (valueArray.Length == 0) {
            return default;
        }
        
        if (valueArray.Length == 1) {
            return valueArray[0];
        }

        StackPanel content = new() {
            Spacing = 5
        };

        RadioButton[] targets = new RadioButton[valueArray.Length];

        for (int i = 0; i < valueArray.Length; i++) {
            T value = valueArray[i];
            targets[i] = new() {
                IsChecked = i == 0,
                GroupName = nameof(valueArray),
                Tag = value,
            };
        }

        TaskDialog dialog = new() {
            Title = SystemMsg.PickOneDialogTitle,
            Content = content,
            Buttons = {
                TaskDialogButton.OKButton,
                TaskDialogButton.CancelButton
            },
            XamlRoot = XamlRoot
        };

        if (await dialog.ShowAsync() is true) {
            return (T?)targets
                .Select(x => (Target: x, x.Tag))
                .FirstOrDefault(x => x.Target.IsChecked is true)
                .Tag;
        }

        return default;
    }
}
