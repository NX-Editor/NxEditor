namespace NxEditor.Component;

public class ReactiveSingleton<T> : ReactiveObject where T : new()
{
    public static T Shared { get; } = new();
}
