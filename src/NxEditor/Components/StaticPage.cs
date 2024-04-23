using Avalonia.Controls;
using Dock.Model.Mvvm.Controls;
using NxEditor.PluginBase.Extensions;
using NxEditor.PluginBase;

namespace NxEditor.Components;

public partial class StaticPage<T, TView> : Document, IStaticPage<T, TView> where T : notnull, new() where TView : Control, new()
{
    public TView View { get; }

    T IStaticPage<T, TView>.Shared => Shared;
    public static T Shared { get; } = new();

    public override void OnSelected()
    {
        if (EditorExtension.LastEditorMenu is not null) {
            Frontend.Locate<IMenuFactory>()
                .Remove(EditorExtension.LastEditorMenu);
        }
    }

    public StaticPage()
    {
        View = new() {
            DataContext = this
        };
    }
}
