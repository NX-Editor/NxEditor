using NxEditor.Plugin.Generics;
using NxEditor.Plugin.Models;

namespace NxEditor.Plugin;

public interface IServiceManager
{
    public void RequestService(IFileHandle handle);
    public void Register(IProcessingService dataProcessor);
    public void Register(IFormatService formatService);
    public void Register(IConfigService configService);
}
