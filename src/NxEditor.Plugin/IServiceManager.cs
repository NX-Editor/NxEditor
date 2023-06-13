using NxEditor.Plugin.Models;
using NxEditor.Plugin.Services;

namespace NxEditor.Plugin;

public interface IServiceManager
{
    public IFormatService RequestService(IFileHandle handle);
    public IServiceManager Register(IProcessingService dataProcessor);
    public IServiceManager Register(IFormatService formatService);
    public IServiceManager Register(IConfigService configService);
}
