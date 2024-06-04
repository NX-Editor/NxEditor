namespace NxEditor.Core.Localization;

public class StringResources_Exceptions
{
    private const string GROUP = "Exception";

    public readonly PlatformNotSupportedException TargetPlatformNotSupported = new(
        GetStringResource(GROUP, nameof(TargetPlatformNotSupported))
    );

    public readonly NotSupportedException AppThemeNotSupported = new(
        GetStringResource(GROUP, nameof(AppThemeNotSupported))
    );

    public readonly ArgumentException UnexpectedViewType = new(
        GetStringResource(GROUP, nameof(UnexpectedViewType))
    );
}
