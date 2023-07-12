using Avalonia.Controls;
using Dock.Model.Mvvm.Controls;

namespace NxEditor.Components;

public partial class StaticPage<T, TView> : Document, IStaticPage<T, TView> where T : notnull, new() where TView : Control, new()
{
    public TView View { get; }

    T IStaticPage<T, TView>.Shared => Shared;
    public static T Shared { get; } = new();

    public StaticPage()
    {
        View = new() {
            DataContext = this
        };
    }
}
