﻿namespace NxEditor.Core.Localization;

public class StringResources_System
{
    private const string GROUP = "System";

    public string Title { get; } = GetStringResource(GROUP, nameof(Title));
    public string Home { get; } = GetStringResource(GROUP, nameof(Home));
    public string VersionNotFound { get; } = GetStringResource(GROUP, nameof(VersionNotFound));
}
