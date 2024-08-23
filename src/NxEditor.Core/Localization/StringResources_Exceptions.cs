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

    private readonly string _tooManyMenuMethodParameters = GetStringResource(GROUP, nameof(TooManyMenuMethodParameters));
    public Exception TooManyMenuMethodParameters(string targetMethodName) => new(
        string.Format(_tooManyMenuMethodParameters, targetMethodName)
    );

    private readonly string _invalidMenuMethodReturnType = GetStringResource(GROUP, nameof(InvalidMenuMethodReturnType));
    public Exception InvalidMenuMethodReturnType(string targetMethodName) => new(
        string.Format(_invalidMenuMethodReturnType, targetMethodName)
    );
}
