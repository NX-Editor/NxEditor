using Avalonia.Controls;

namespace NxEditor.Components;

public interface IStaticPage<T, TView> : IStaticPage where T : notnull, new() where TView : UserControl, new()
{
    UserControl IStaticPage.View => View;
    object IStaticPage.Shared => Shared;
    public new TView View { get; }
    public new T Shared { get; }
}

public interface IStaticPage
{
    public object Shared { get; }
    public UserControl View { get; }
}