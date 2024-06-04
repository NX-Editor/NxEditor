using Avalonia.Controls;

namespace NxEditor.Components;

public static class ViewRepository
{
    private static readonly Dictionary<object, object> _views = [];

    public static TView Get<TView>(object viewModel) where TView : Control
    {
        if (!_views.TryGetValue(viewModel, out object? view)) {
            string viewTypeName = viewModel.GetType().AssemblyQualifiedName?.Replace("ViewModel", "View")
                ?? throw Exceptions.UnexpectedViewType;

            Type viewType = Type.GetType(viewTypeName, throwOnError: true)
                ?? throw Exceptions.UnexpectedViewType;

            _views[viewModel] = view = Activator.CreateInstance(viewType)
                ?? throw Exceptions.UnexpectedViewType;
        }

        return view as TView
            ?? throw Exceptions.UnexpectedViewType;
    }

    public static bool Release<TViewModel>()
    {
        return _views.Remove(typeof(TViewModel));
    }
}
