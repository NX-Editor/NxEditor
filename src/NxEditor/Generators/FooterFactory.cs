using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using CommunityToolkit.Mvvm.Input;
using NxEditor.PluginBase.Attributes;
using Projektanker.Icons.Avalonia;
using System.Collections.ObjectModel;
using System.Reflection;

namespace NxEditor.Generators;

public class FooterFactory : IFooterFactory
{
    private readonly Dictionary<Type, List<Control>> _groups = [];

    public ObservableCollection<Control> Items { get; set; } = [];

    public IFooterFactory Append<T>(T source) where T : class
    {
        return Append((object)source);
    }

    public IFooterFactory Append(object source)
    {
        Type type = source.GetType();
        List<Control> group = _groups[type] = [];
        IEnumerable<(MethodInfo, FooterAttribute)> methods = type
            .GetMethods()
            .Select(x => (x, footer: x.GetCustomAttribute<FooterAttribute>()!))
            .Where(x => x.footer is not null);

        foreach (var (methodInfo, footerInfo) in methods) {
            AsyncRelayCommand command = new(async () => {
                if (methodInfo.Invoke(source, []) is Task task) {
                    await task;
                }
            });

            Button button = new() {
                Width = 23,
                Height = 23,
                Padding = new(1),
                Margin = new(1),
                Background = Brushes.Transparent,
                Content = new Icon {
                    Value = footerInfo.Icon
                },
                Command = command
            };

            ToolTip.SetTip(button, footerInfo.Description);

            group.Add(button);
            Items.Add(button);

            if (footerInfo.IsSeparator) {
                Separator separator = new() {
                    Height = 15,
                    Margin = new(3, 5, 3, 5),
                    VerticalAlignment = VerticalAlignment.Center,
                    Width = 1
                };

                group.Add(separator);
                Items.Add(separator);
            }
        }

        return this;
    }

    public IFooterFactory Remove<T>() where T : class
    {
        return Remove(typeof(T));
    }

    public IFooterFactory Remove(Type type)
    {
        if (_groups.TryGetValue(type, out List<Control>? group)) {
            return this;
        }

        foreach (Control item in _groups[type]) {
            Items.Remove(item);
        }

        return this;
    }
}
