using Cead;
using ExKingEditor.Models;

namespace ExKingEditor.ViewModels.Editors;

public partial class BymlViewModel : ReactiveEditor
{
    public readonly string _yaml;
    public string Yaml { get; set; }

    public BymlViewModel(string file) : base(file)
    {
        // Load source into readonly field for diffing
        using Byml byml = Byml.FromBinary(RawData());
        _yaml = byml.ToText();

        Yaml = _yaml;
    }

    public override void SaveEditor()
    {
        throw new NotImplementedException();
    }

    public override bool HasChanged()
    {
        return _yaml != Yaml;
    }
}
