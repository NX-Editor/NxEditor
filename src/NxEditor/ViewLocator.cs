using Avalonia.Controls;
using Avalonia.Controls.Templates;
using CommunityToolkit.Mvvm.ComponentModel;
using NxEditor.PluginBase.Component;

namespace NxEditor;

public class ViewLocator : IDataTemplate
{
    public Control? Build(object? param)
    {
        if (param is IEditor editor) {
            return editor.View;
        }

        var name = param!.GetType().FullName!.Replace("ViewModel", "View");
        var type = Type.GetType(name);

        if (type != null) {
            return (Control)Activator.CreateInstance(type)!;
        }
        else {
            return new TextBlock { Text = "Not Found: " + name };
        }
    }

    public bool Match(object? data)
    {
        return data is ReactiveObject or ObservableObject;
    }
}
