using Avalonia.Controls;
using Avalonia.Controls.Templates;

namespace NxEditor.Components;

public class ViewLocator : IDataTemplate
{
    public Control? Build(object? target)
    {
        return target switch {
            ObservableObject vm => ViewRepository.Get<Control>(vm),
            _ => null
        };
    }

    public bool Match(object? data)
    {
        return data is ObservableObject;
    }
}
