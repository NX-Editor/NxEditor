using Avalonia.Controls;
using Avalonia.Input;
using CommunityToolkit.Mvvm.Input;
using NxEditor.PluginBase.Attributes;
using System.Collections;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace NxEditor.Generators;

public class MenuFactory : IMenuFactory
{
    private readonly TopLevel? _topLevel;
    private readonly Dictionary<Type, List<Control>> _groups = new();
    private ItemsControl? _parent;

    public ObservableCollection<Control> Items { get; set; } = new();

    public MenuFactory(TopLevel? topLevel)
    {
        _topLevel = topLevel;
    }

    /// <summary>
    /// Removes all items added by the provided type (<typeparamref name="T"/>)
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public IMenuFactory Prepend<T>() where T : class
    {
        if (_groups.TryGetValue(typeof(T), out List<Control>? group)) {
            foreach (var item in group) {
                if (item.Parent is ItemsControl itemsControl) {
                    try {
                        itemsControl.Items.Remove(item);
                    }
                    catch {
                        Items.Remove(item);
                    }
                }
            }

            _groups.Remove(typeof(T));
        }

        return this;
    }

    /// <summary>
    /// Creates the menu stack from the provided <paramref name="source"/> model.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="source"></param>
    public IMenuFactory Append<T>(T source) where T : class
    {
        List<Control> group = _groups[typeof(T)] = new();
        foreach ((var info, var attribute) in Collect<T>()) {
            AppendItem(source, info, attribute, group);
            _parent = null;
        }

        return this;
    }

    private void AppendItem<T>(T source, MethodInfo info, MenuAttribute attribute, List<Control> group) where T : class
    {
        SetParentFromPath(attribute.Path, group);

        KeyGesture? hotKey = string.IsNullOrEmpty(attribute.HotKey)
            ? null : KeyGesture.Parse(attribute.HotKey);

        if (attribute.IsSeparator) {
            Separator separator = new();
            _parent?.Items.Add(separator);
            group.Add(separator);
        }

        MenuItem item = new() {
            Header = attribute.Name,
            Icon = new Projektanker.Icons.Avalonia.Icon {
                Value = attribute.Icon
            },
            Classes = {
                "MenuFactor-MenuItem"
            },
            InputGesture = hotKey
        };

        _parent?.Items.Add(item);
        group.Add(item);

        if (typeof(T).GetMethod(attribute.GetCollectionMethodName ?? "")?.Invoke(source, null) is ObservableCollection<MenuItem> collection) {
            item.ItemsSource = collection;
            collection.CollectionChanged += (s, e) => {
                if (e.NewItems is IList items) {
                    ProcessNewItems(source, items, info);
                }
            };

            return;
        }

        item.Command = new AsyncRelayCommand(async () => {
            if (info.Invoke(source, null) is Task task) {
                await task;
            }
        });

        if (hotKey != null) {
            _topLevel?.RegisterHotKey(hotKey, attribute.Path, item.Command);
        }
    }

    private static (MethodInfo, MenuAttribute)[] Collect<T>() where T : class
    {
        return typeof(T).GetMethods()
            .Select(x => (x, attribute: x.GetCustomAttribute<MenuAttribute>()!))
            .Where(x => x.attribute != null)
            .ToArray();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void ProcessNewItems<T>(T source, IList items, MethodInfo info) where T : class
    {
        foreach (var item in items) {
            if (item is MenuItem menuItem) {
                menuItem.Command = new AsyncRelayCommand(async () => {
                    if (info.Invoke(source, new object?[] { menuItem }) is Task task) {
                        await task;
                    }
                });
            }
        }
    }

    private void SetParentFromPath(string path, List<Control> group)
    {
        foreach (var part in path.Replace('\\', '/').Split('/')) {
            MenuItem item = new() {
                Header = part,
                Classes = {
                    "MenuFactor-MenuItem"
                }
            };

            if (_parent == null) {
                item.Classes.Add("MenuFactor-TopLevel");
            }

            IEnumerable<object> source = (IEnumerable<object>?)_parent?.Items ?? Items;
            if (source.Select(x => x as MenuItem).Where(x => x != null && (string?)x.Header == part).FirstOrDefault() is MenuItem existingItem) {
                _parent = existingItem;
                continue;
            }

            (source as IList)?.Add(_parent = item);
            group.Add(item);
        }
    }
}
