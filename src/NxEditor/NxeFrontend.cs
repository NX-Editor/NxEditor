using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using FluentAvalonia.UI.Controls;
using NxEditor.Components;
using NxEditor.Core.Components;

namespace NxEditor;

public class NxeFrontend(Visual xamlRoot) : INxeFrontend
{
    public Visual XamlRoot { get; } = xamlRoot;
    public IMenuFactory MenuFactory { get; } = new MenuFactory();

    public async ValueTask<T?> PickOneAsync<T>(IAsyncEnumerable<T> values, string? footerContent = null)
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
            RadioButton target = targets[i] = new() {
                IsChecked = i == 0,
                Content = value?.ToString(),
                GroupName = nameof(valueArray),
                Tag = value,
            };

            content.Children.Add(target);
        }

        TaskDialog dialog = new() {
            Title = SystemMsg.PickOneDialogTitle,
            Content = content,
            Buttons = {
                TaskDialogButton.OKButton,
                TaskDialogButton.CancelButton
            },
            Footer = new TextBlock {
                Text = footerContent,
                FontSize = 12,
                TextWrapping = TextWrapping.WrapWithOverflow
            },
            FooterVisibility = footerContent switch {
                null => TaskDialogFooterVisibility.Never,
                _ => TaskDialogFooterVisibility.Always
            },
            XamlRoot = XamlRoot
        };

        if (await dialog.ShowAsync() is TaskDialogStandardResult.OK) {
            return (T?)targets
                .Select(x => (Target: x, x.Tag))
                .FirstOrDefault(x => x.Target.IsChecked is true)
                .Tag;
        }

        return default;
    }
}
