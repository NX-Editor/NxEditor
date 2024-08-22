namespace NxEditor.Core.Localization;

public class StringResources_System
{
    private const string GROUP = "System";

    public string Title { get; } = GetStringResource(GROUP, nameof(Title));
    public string Home { get; } = GetStringResource(GROUP, nameof(Home));
    public string HomeDragDropEdit { get; } = GetStringResource(GROUP, nameof(HomeDragDropEdit));
    public string VersionNotFound { get; } = GetStringResource(GROUP, nameof(VersionNotFound));
    public string PickOneDialogTitle { get; } = GetStringResource(GROUP, nameof(PickOneDialogTitle));
}
