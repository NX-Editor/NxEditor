using Avalonia.Controls;
using Avalonia.Input;
using CommunityToolkit.Mvvm.Input;
using NxEditor.Attributes;
using NxEditor.Components;
using NxEditor.Models;
using NxEditor.Views;
using System.Collections.ObjectModel;
using System.Reflection;

namespace NxEditor.Generators;

public class MenuFactory
{
    private static ShellView? _visualRoot;
    public static ObservableCollection<Control>? ItemsSource { get; set; }

    public static void Init(ShellView visualRoot)
    {
        _visualRoot = visualRoot;
    }

    public static ObservableCollection<Control> Generate<T>() => Generate<T>(default);
    public static ObservableCollection<Control> Generate<T>(T? model)
    {
        // Filter the ViewModel functions
        IEnumerable<MethodInfo> functions = typeof(T).GetMethods().Where(x => x.GetCustomAttributes<MenuAttribute>().Any());

        // Define a new dynamic dictionary object to
        // store the sorted menu functions
        Dictionary<string, dynamic> pseudoMenu = new();

        // Iterate the sorted functions and organize
        // functions by the Path property
        foreach (var func in functions) {

            // Store the menu attribute data
            var menu = func.GetCustomAttribute<MenuAttribute>()!;

            // Null check the path
            if (menu.Path == null) {
                pseudoMenu.Add(func.Name, func);
                continue;
            }

            // Store the current dictionary position
            Dictionary<string, dynamic> dictPos = pseudoMenu;

            // Split the path by '.' as a separator
            // and create the required dicts for
            // each "folder" in the path
            foreach (var folder in menu.Path.Split('/')) {
                if (!dictPos.ContainsKey(folder)) {
                    dictPos.Add(folder, new Dictionary<string, dynamic>());
                }

                dictPos = dictPos[folder];
            }

            // Add the last item in the tree to
            // the current dictionary
            dictPos.Add(func.Name, func);
        }

        return ItemsSource = SetChildItems(pseudoMenu, model);
    }

    private static ObservableCollection<Control> SetChildItems<T>(Dictionary<string, dynamic> data, T? obj)
    {
        // Define the root list
        ObservableCollection<Control> itemsRoot = new();

        foreach ((var name, var childData) in data) {
            MenuItem child;

            if (childData is MethodInfo func) {

                var menu = func.GetCustomAttribute<MenuAttribute>()!;
                if (menu.IsSeparator) {
                    itemsRoot.Add(new Separator());
                }

                KeyGesture? shortcut = string.IsNullOrEmpty(menu.HotKey) ? null : KeyGesture.Parse(menu.HotKey);

                var command = new RelayCommand(() => {
                    try {
                        if (ShellView.MainMenu?.Any(x => x.Name == $"MenuItem__{menu.PathRoot()}") == true) {
                            func.Invoke(obj, Array.Empty<object>());
                        }
                    }
                    catch (Exception ex) {
                        App.Log(ex, func.Name, "[ShellMenu]", -1);
                    }
                });

                child = new() {
                    Header = menu.Name ?? func.Name,
                    Icon = new Projektanker.Icons.Avalonia.Icon() { Value = menu.Icon },
                    InputGesture = shortcut!,
                    Height = (menu.Name ?? func.Name).StartsWith("_") ? 30 : double.NaN,
                    Classes = {
                        "MenuFactor-MenuItem"
                    },
                    Command = command
                };

                if (shortcut != null) {
                    _visualRoot?.RegisterHotKey(shortcut, menu.Path, command);
                }

                if (func.Name == "Recent") {
                    child.ItemsSource = RecentFiles.Shared;
                    RecentFiles.Shared.CollectionChanged += (s, e) => {
                        if (e.NewItems != null) {
                            foreach (var item in e.NewItems) {
                                if (item is MenuItem menuItem) {
                                    menuItem.Command = new RelayCommand(() => {
                                        func.Invoke(obj, new object?[] {
                                            ToolTip.GetTip(menuItem)
                                        });
                                    });
                                }
                            }
                        }
                    };
                }
            }
            else if (childData is Dictionary<string, dynamic> dict) {
                child = new() {
                    Name = $"MenuItem__{name}",
                    Header = name,
                    ItemsSource = SetChildItems(dict, obj),
                    Classes = {
                        "MenuFactor-TopLevel",
                        "MenuFactor-MenuItem"
                    }
                };
            }
            else {
                throw new ArgumentException($"The passed type '{data.GetType()}' was not in the expected format.");
            }

            itemsRoot.Add(child);
        }

        return itemsRoot;
    }
}