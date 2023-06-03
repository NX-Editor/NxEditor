using Avalonia;
using Avalonia.Controls;
using AvaloniaEdit;
using AvaloniaEdit.TextMate;
using ExKingEditor.ViewModels.Editors;
using TextMateSharp.Grammars;

namespace ExKingEditor.Views.Editors;

public partial class TextView : UserControl
{
    private static readonly RegistryOptions _registryOptions = new(ThemeName.DarkPlus);
    private static string _grammerId = string.Empty;

    public TextView()
    {
        InitializeComponent();
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

        if (DataContext is TextViewModel vm) {
            vm.Editor = TextEditor;
            TextEditor.Text = vm.Text;

            if (_registryOptions.GetLanguageByExtension(Path.GetExtension(vm.FilePath))?.Id is string id) {
                _grammerId = _registryOptions.GetScopeByLanguageId(id);
                TextMate.Installation textMateInstallation = TextEditor.InstallTextMate(_registryOptions);
                textMateInstallation.SetGrammar(_grammerId);
            }

            TextEditor.TextChanged += (s, e) => {
                vm.Text = TextEditor.Text;
            };
        }
    }
}
