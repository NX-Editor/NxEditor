using System.Diagnostics.CodeAnalysis;

namespace NxEditor.Attributes;

[AttributeUsage(AttributeTargets.Method)]
public class MenuAttribute : Attribute
{
    public required string Name { get; set; }
    public required string Path { get; set; }
    public string HotKey { get; set; }
    public string? Icon { get; set; }
    public bool IsSeparator { get; set; } = false;

    [SetsRequiredMembers]
    public MenuAttribute(string name, string path, string? hotkey = null, string? icon = null)
    {
        Name = name;
        Path = path;
        HotKey = hotkey ?? string.Empty;
        Icon = icon;
    }

    public string PathRoot()
    {
        int idx = Path.IndexOf('/');
        return idx > -1 ? Path[..idx] : Path;
    }
}