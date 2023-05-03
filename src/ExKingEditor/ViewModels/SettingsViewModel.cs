using Dock.Model.Mvvm.Controls;
using ExKingEditor.Views;

namespace ExKingEditor.ViewModels;

public class SettingsViewModel : Document
{
    public SettingsViewModel()
    {
        Id = nameof(SettingsViewModel);
        Title = "Settings";
        CanFloat = false;
    }

    public override bool OnClose()
    {
        SettingsView.Live?.ValidateSave();
        SettingsView.Live = null;
        return base.OnClose();
    }
}
