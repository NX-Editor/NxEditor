using Avalonia;
using Avalonia.Controls;
using AvaloniaEdit;
using AvaloniaEdit.TextMate;
using NxEditor.ViewModels.Editors;
using TextMateSharp.Grammars;

namespace NxEditor.Views.Editors;

public partial class BymlView : UserControl
{
    private static readonly RegistryOptions _registryOptions = new(ThemeName.DarkPlus);

    public BymlView()
    {
        InitializeComponent();

        // Initialize TextEditor
        TextMate.Installation textMateInstallation = TextEditor.InstallTextMate(_registryOptions);
        textMateInstallation.SetGrammar("source.yaml");
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        App.Desktop?.DisableGlobalShortcuts("Edit");
        base.OnAttachedToVisualTree(e);
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        App.Desktop?.ActivateGlobalShortcuts();
        base.OnDetachedFromVisualTree(e);
    }

    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);

        if (DataContext is BymlViewModel vm) {
            vm.Editor = TextEditor;
            TextEditor.Text = vm.Yaml;
            TextEditor.TextChanged += (s, e) => {
                vm.Yaml = TextEditor.Text;
            };
        }
    }
}
