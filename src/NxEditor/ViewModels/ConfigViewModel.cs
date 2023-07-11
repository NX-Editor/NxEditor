using Avalonia.Controls.Notifications;
using ConfigFactory;
using ConfigFactory.Avalonia;
using ConfigFactory.Core;
using ConfigFactory.Models;

namespace NxEditor.ViewModels;

public class ConfigViewModel : ConfigPageModel, IStaticPage<ConfigViewModel, ConfigPage>
{
    public ConfigPage View { get; }
    public List<IConfigModule> ConfigModules { get; set; } = new();
    public bool IsValid { get; set; } = true;

    ConfigViewModel IStaticPage<ConfigViewModel, ConfigPage>.Shared => Shared;
    public static ConfigViewModel Shared { get; } = new();

    public ConfigViewModel()
    {
        Id = nameof(ConfigViewModel);
        Title = "Settings";
        CanFloat = false;

        View = new() {
            DataContext = this
        };

        this.Append(Config.Shared);
        SecondaryButtonIsEnabled = false;

        PrimaryButtonCompletedEvent += () => {
            ValidateModules();
            return Task.CompletedTask;
        };
    }

    public override void OnSelected()
    {
        CanClose = IsValid;
    }

    public override bool OnClose()
    {
        // Reset each config entry to its saved
        // value in case a value is invalid and
        // the user closes the document.
        foreach (var module in ConfigModules) {
            IConfigModule config = module.Load();
            foreach ((var name, (var property, _)) in config.Properties) {
                property.SetValue(module.Shared, config.Properties[name].info.GetValue(config));
            }
        }

        return base.OnClose();
    }

    public bool ValidateModules()
    {
        foreach (var module in ConfigModules) {
            if (!module.Validate(out string? msg)) {
                App.Toast(msg ?? "Please review your settings", $"Invalid Configuration", NotificationType.Error);
                return IsValid = CanClose = false;
            }
        }

        return IsValid = CanClose = true;
    }
}
