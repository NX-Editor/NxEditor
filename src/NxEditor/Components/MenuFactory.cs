using Avalonia.Controls;
using Avalonia.Input;
using CommunityToolkit.Mvvm.Input;
using NxEditor.Core.Attributes;
using NxEditor.Core.Components;
using System.Collections;
using System.Collections.ObjectModel;
using System.Reflection;
using Icon = Projektanker.Icons.Avalonia.Icon;

namespace NxEditor.Components;

public class MenuFactory : IMenuFactory
{
    private const string MENU_ITEM_STYLE = "MenuFactory-MenuItem";
    private const string TOP_LEVEL_MENU_ITEM_STYLE = "MenuFactory-TopLevel";

    private readonly Dictionary<string, List<MenuItem>> _groups = [];
    private readonly Dictionary<string, MenuItem> _cache = [];
    private readonly ObservableCollection<MenuItem> _items = [];

    public IEnumerable Items => _items;

    public void AddMenuGroup<T>(string groupId, object? source = null)
    {
        Type type = typeof(T);

        IEnumerable<(MethodInfo, MenuAttribute)> attributes = type
            .GetMethods(BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            .Select(x => (MethodInfo: x, Attribute: x.GetCustomAttribute<MenuAttribute>()!))
            .Where(x => x.MethodInfo is not null && x.Attribute is not null);

        if (!_groups.TryGetValue(groupId, out List<MenuItem>? groupItems)) {
            _groups[groupId] = groupItems = [];
        }

        foreach (var (info, attribute) in attributes) {
            groupItems.Add(
                BuildMenuItemFromAttribute(info, attribute, source)
            );
        }
    }

    public bool RemoveMenuGroup(string groupId)
    {
        if (!_groups.TryGetValue(groupId, out List<MenuItem>? items)) {
            return false;
        }

        for (int i = 0; i < items.Count; i++) {
            ItemsControl item = items[i];

        RemoveFromParent:
            if (item.Parent is ItemsControl parent) {
                parent.Items.Remove(item);

                if (parent.ItemCount == 0) {
                    item = parent;
                    goto RemoveFromParent;
                }
            }
        }

        return true;
    }

    private MenuItem BuildMenuItemFromAttribute(MethodInfo info, MenuAttribute attribute, object? source)
    {
        if (info.GetParameters().Length > 0) {
            throw Exceptions.TooManyMenuMethodParameters(info.Name);
        }

        if (info.ReturnType.IsAssignableTo(typeof(ValueTask<>))) {
            throw Exceptions.InvalidMenuMethodReturnType(info.Name);
        }

        AsyncRelayCommand command = new(async () => {
            object? result = info.Invoke(source, null);

            if (result is Task task) {
                await task;
                return;
            }

            if (result is ValueTask valueTask) {
                await valueTask;
                return;
            }
        });

        KeyGesture? inputGesture = string.IsNullOrEmpty(attribute.InputGesture)
            ? null : KeyGesture.Parse(attribute.InputGesture);

        MenuItem result = new() {
            Header = attribute.Name,
            Command = command,
            InputGesture = inputGesture,
            Classes = {
                MENU_ITEM_STYLE
            },
            Icon = new Icon {
                Value = attribute.Icon ?? string.Empty
            }
        };

        MenuItem? parent = BuildMenuItemPath(attribute.Path);

        if (parent is null) {
            _items.Add(result);
        }
        else {
            parent.Items.Add(result);
        }

        return result;
    }

    private MenuItem? BuildMenuItemPath(string path)
    {
        IEnumerable<PathPart> parts = path
            .Split('/')
            .Select(x => new PathPart(x))
            .Reverse();

        MenuItem? deepestMenuItem = null;
        MenuItem? highestMenuItem = null;
        foreach (PathPart part in parts) {
            if (!_cache.TryGetValue(part.Name, out MenuItem? item)) {
                item = _cache[part.Name] = new() {
                    Header = part.Name,
                    Icon = new Icon {
                        Value = part.Icon ?? string.Empty
                    }
                };
            }

            if (highestMenuItem is not null && !item.Items.Contains(highestMenuItem)) {
                item.Items.Add(highestMenuItem);
            }

            highestMenuItem = item;
            deepestMenuItem ??= item;
        }

        if (highestMenuItem is not null && !_items.Contains(highestMenuItem)) {
            _items.Add(highestMenuItem);
            highestMenuItem?.Classes.Add(TOP_LEVEL_MENU_ITEM_STYLE);
        }

        return deepestMenuItem;
    }

    private class PathPart
    {
        public string Name { get; }
        public string? Icon { get; }
        public bool IsSeparator { get; } = false;

        public PathPart(string input)
        {
            ReadOnlySpan<char> inputSpan = input.AsSpan();

            int paramsStartIndex = inputSpan.IndexOf("$:");
            if (paramsStartIndex == -1) {
                Name = input;
                return;
            }

            Name = input[..paramsStartIndex];
            paramsStartIndex += 2;

            const string ICON_KEYWORD = "Icon";
            const string IS_SEPARATOR_KEYWORD = "IsSeparator";

        MoveNext:
            if (inputSpan.Length <= paramsStartIndex) {
                return;
            }

            int nextCommaIndex = inputSpan[paramsStartIndex..].IndexOf(',');
            int parameterValueEndIndex = nextCommaIndex switch {
                -1 => inputSpan.Length,
                _ => paramsStartIndex + nextCommaIndex
            };

            if (inputSpan[paramsStartIndex..(paramsStartIndex + ICON_KEYWORD.Length)] is ICON_KEYWORD) {
                Icon = input[(paramsStartIndex + ICON_KEYWORD.Length + 1)..parameterValueEndIndex];
                goto MoveNextSetup;
            }

            if (inputSpan[paramsStartIndex..(paramsStartIndex + IS_SEPARATOR_KEYWORD.Length)] is IS_SEPARATOR_KEYWORD) {
                IsSeparator = bool.Parse(
                    input[(paramsStartIndex + IS_SEPARATOR_KEYWORD.Length + 1)..parameterValueEndIndex]
                );
                goto MoveNextSetup;
            }

        MoveNextSetup:
            paramsStartIndex = parameterValueEndIndex + 1;
            goto MoveNext;
        }
    }
}
