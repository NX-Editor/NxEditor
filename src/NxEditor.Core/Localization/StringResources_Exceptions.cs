namespace NxEditor.Core.Localization;

public class StringResources_Exceptions
{
    private const string GROUP = "Exception";

    public PlatformNotSupportedException TargetPlatformNotSupported { get; } = new(
        GetStringResource(GROUP, nameof(TargetPlatformNotSupported))
    );

    public NotSupportedException AppThemeNotSupported { get; } = new(
        GetStringResource(GROUP, nameof(AppThemeNotSupported))
    );

    public ArgumentException UnexpectedViewType { get; } = new(
        GetStringResource(GROUP, nameof(UnexpectedViewType))
    );

    public Exception NoFrontendException { get; } = new(
        GetStringResource(GROUP, nameof(NoFrontendException))
    );
}
