﻿using Avalonia.Controls;
using Avalonia.Input;
using CommunityToolkit.Mvvm.Input;
using NxEditor.PluginBase;
using NxEditor.PluginBase.Attributes;
using System.Collections;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace NxEditor.Generators;

public class MenuFactory(TopLevel? topLevel) : IMenuFactory
{
    private readonly TopLevel? _topLevel = topLevel;
    private readonly Dictionary<Type, List<Control>> _groups = [];
    private ItemsControl? _parent;

    public ObservableCollection<Control> Items { get; set; } = [];

    /// <summary>
    /// Removes all items added by the provided type
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public IMenuFactory Remove<T>() where T : class
    {
        return Remove(typeof(T));
    }

    /// <inheritdoc cref="Remove{T}"/>
    public IMenuFactory Remove(Type type)
    {
        if (_groups.TryGetValue(type, out List<Control>? group)) {
            foreach (Control item in group) {
                if (item.Parent is ItemsControl itemsControl) {
                    try {
                        itemsControl.Items.Remove(item);
                    }
                    catch {
                        Items.Remove(item);
                    }
                }
            }

            _groups.Remove(type);
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
        return Append((object)source);
    }

    /// <inheritdoc cref="Append{T}(T)"/>
    public IMenuFactory Append(object source)
    {
        Type type = source.GetType();
        List<Control> group = _groups[type] = [];
        foreach ((MethodInfo? info, MenuAttribute? attribute) in Collect(type)) {
            AppendItem(type, source, info, attribute, group);
            _parent = null;
        }

        return this;
    }

    private void AppendItem(Type type, object source, MethodInfo info, MenuAttribute attribute, List<Control> group)
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
                Value = attribute.Icon ?? "fa-solid fa-crow"
            },
            Classes = {
                "MenuFactor-MenuItem"
            },
            InputGesture = hotKey
        };

        _parent?.Items.Add(item);
        group.Add(item);

        if (type.GetMethod(attribute.GetCollectionMethodName ?? "")?.Invoke(source, null) is ObservableCollection<MenuItem> collection) {
            item.ItemsSource = collection;
            collection.CollectionChanged += (s, e) => {
                if (e.NewItems is IList items) {
                    ProcessNewItems(source, items, info);
                }
            };

            return;
        }

        item.Command = new AsyncRelayCommand(async () => {
            try {
                if (info.Invoke(source, null) is Task task) {
                    await task;
                }
            }
            catch (Exception ex) {
                StatusModal.Set($"Failed to execute action: {attribute.Name}", "fa-regular fa-circle-xmark", isWorkingStatus: false, temporaryStatusTime: 3);
                Trace.WriteLine($"{ex} - {ex.Message}");
            }
        });

        if (hotKey != null) {
            _topLevel?.RegisterHotKey(hotKey, attribute.Path, item.Command);
        }
    }

    private static (MethodInfo, MenuAttribute)[] Collect(Type type)
    {
        return type.GetMethods()
            .Select(x => (x, attribute: x.GetCustomAttribute<MenuAttribute>()!))
            .Where(x => x.attribute != null)
            .ToArray();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void ProcessNewItems(object source, IList items, MethodInfo info)
    {
        foreach (object? item in items) {
            if (item is MenuItem menuItem) {
                menuItem.Command = new AsyncRelayCommand(async () => {
                    if (info.Invoke(source, [menuItem]) is Task task) {
                        await task;
                    }
                });
            }
        }
    }

    private void SetParentFromPath(string path, List<Control> group)
    {
        foreach (string part in path.Replace('\\', '/').Split('/')) {
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
