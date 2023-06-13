namespace NxEditor.Plugin.Attributes;

[AttributeUsage(AttributeTargets.Method)]
public class CommandAttribute : Attribute
{
    /// <summary>
    /// The name used in the CLI to call this command (should not contain spaces)
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// The description of this command displayed in the help command
    /// </summary>
    public required string Description { get; set; }
}
