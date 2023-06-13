namespace NxEditor.Plugin;

/// <summary>
/// Public extension interface
/// </summary>
public interface IServiceExtension
{
    public string Name { get; }
    public void RegisterExtension(IServiceManager serviceManager);
}
