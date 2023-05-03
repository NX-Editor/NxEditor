using Avalonia.Controls;
using Avalonia.Input;
using ExKingEditor.Attributes;
using System.Reflection;

namespace ExKingEditor.Generators;

public class MenuFactory
{
    /// <summary>
    /// Defaults:<br/>
    /// - <i>MenuFactor-TopLevel</i><br/>
    /// - <i>MenuFactor-MenuItem</i>
    /// </summary>
    public static Classes TopLevelClasses { get; set; } = new() {
        "MenuFactor-TopLevel",
        "MenuFactor-MenuItem"
    };

    /// <summary>
    /// Defaults:<br/>
    /// - <i>MenuFactor-MenuItem</i>
    /// </summary>
    public static Classes MenuItemClasses { get; set; } = new() {
        "MenuFactor-MenuItem"
    };

    public static List<Control> Generate<T>() where T : new() => Generate(new T());
    public static List<Control> Generate<T>(T model)
    {
        if (model == null) {
            throw new ArgumentNullException(nameof(model), "Cannot generate menu from null object");
        }

        // Filter the ViewModel functions
        IEnumerable<MethodInfo> functions = model.GetType().GetMethods().Where(x => x.GetCustomAttributes<MenuAttribute>().Any());

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
                if (!dictPos.ContainsKey(folder)) dictPos.Add(folder, new Dictionary<string, dynamic>());
                dictPos = dictPos[folder];
            }

            // Add the last item in the tree to
            // the current dictionary
            dictPos.Add(func.Name, func);
        }

        return CollectChildItems(pseudoMenu, model);
    }

    private static List<Control> CollectChildItems<T>(Dictionary<string, dynamic> data, T obj)
    {
        // Define the root list
        List<Control> itemsRoot = new();

        foreach ((var name, var childData) in data) {
            MenuItem child;

            if (childData is MethodInfo func) {
                var menu = func.GetCustomAttribute<MenuAttribute>()!;
                if (menu.IsSeparator) itemsRoot.Add(new Separator());

                KeyGesture? shortcut = string.IsNullOrEmpty(menu.HotKey) ? null : KeyGesture.Parse(menu.HotKey);

                child = new() {
                    Header = menu.Name ?? func.Name,
                    Icon = new Projektanker.Icons.Avalonia.Icon() { Value = menu.Icon },
                    HotKey = shortcut,
                    InputGesture = shortcut!,
                    Height = (menu.Name ?? func.Name).StartsWith("_") ? 30 : double.NaN,
                    Classes = MenuItemClasses,
                    Command = ReactiveCommand.Create(() => {
                        func.Invoke(obj, Array.Empty<object>());
                    })
                };

                if (shortcut != null) {
                    HotKeyManager.SetHotKey(child, shortcut);
                }

                child.Classes.Add("");
            }
            else if (childData is Dictionary<string, dynamic> dict) {
                child = new() {
                    Header = name,
                    Items = CollectChildItems(dict, obj),
                    Classes = TopLevelClasses
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