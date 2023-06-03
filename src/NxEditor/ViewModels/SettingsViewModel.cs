using Avalonia.Controls.Notifications;
using Dock.Model.Mvvm.Controls;
using NxEditor.Views;

namespace NxEditor.ViewModels;

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
        if (SettingsView.Live?.ValidateSave() is string error) {
            App.Toast(error, "Invalid Settings", NotificationType.Error);
            return false;
        }

        SettingsView.Live = null;
        return base.OnClose();
    }
}
