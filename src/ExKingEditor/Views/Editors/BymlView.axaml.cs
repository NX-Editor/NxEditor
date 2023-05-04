using Avalonia.Controls;
using AvaloniaEdit;
using AvaloniaEdit.TextMate;
using ExKingEditor.ViewModels.Editors;
using TextMateSharp.Grammars;

namespace ExKingEditor.Views.Editors;

public partial class BymlView : UserControl
{
    private static readonly RegistryOptions registryOptions = new(ThemeName.DarkPlus);

    public BymlView()
    {
        InitializeComponent();

        // Initialize TextEditor
        TextMate.Installation textMateInstallation = TextEditor.InstallTextMate(registryOptions);
        textMateInstallation.SetGrammar("source.yaml");
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
