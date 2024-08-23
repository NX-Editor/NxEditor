namespace NxEditor.Core.Attributes;

/// <summary>
/// Apply this method to a function or method and register the parent class with the <see cref="INxeFrontend"/> to add menu items to the <see langword="global"/> menu.
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class MenuAttribute(string name, string path) : Attribute
{

    /// <summary>
    /// The display name of the menu item.
    /// </summary>
    public string Name { get; set; } = name;

    /// <summary>
    /// The path to this menu item. Use a forward slash to separate menu items and $() to define properties.<br/>
    /// <b>Example:</b> File/Settings$:Icon=fa-cog,IsSeparator=true/
    /// </summary>
    public string Path { get; set; } = path;

    /// <summary>
    /// The input gesture (hotkey) that will trigger this menu item.
    /// </summary>
    public string? InputGesture { get; set; }

    /// <summary>
    /// The icon used in the menu item.
    /// </summary>
    public string? Icon { get; set; }
    
    /// <summary>
    /// When <see langword="true"/> a separator bar will be placed above the menu item. Default is <see langword="false"/>.
    /// </summary>
    public bool IsSeparator { get; set; } = false;

    /// <summary>
    /// The name of a method or function that returns a collection of items to fill this item.<br/>
    /// The attributed method will be invoked when a child item is clicked.
    /// </summary>
    public string? GetCollectionMethodName { get; set; }
}
