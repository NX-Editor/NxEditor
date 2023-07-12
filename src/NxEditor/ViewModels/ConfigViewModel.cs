using Avalonia.Controls.Notifications;
using ConfigFactory;
using ConfigFactory.Avalonia;
using ConfigFactory.Core.Attributes;
using ConfigFactory.Core.Models;
using ConfigFactory.Models;

namespace NxEditor.ViewModels;

public class ConfigViewModel : ConfigPageModel, IStaticPage<ConfigViewModel, ConfigPage>
{
    public ConfigPage View { get; }
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
        foreach ((_, var module) in ConfigModules) {
            module.Shared.Reset();
        }

        return base.OnClose();
    }

    public bool ValidateModules()
    {
        foreach ((_, var module) in ConfigModules) {
            if (!module.Validate(out string? msg, out ConfigProperty target)) {
                App.Toast($"""
                    {msg ?? "Please review your settings"}
                    Error at '{target.Attribute.Category}/{target.Attribute.Group}/{target.Attribute.Header}'
                    """, $"Invalid Config", NotificationType.Error);
                FocusGroup(target.Attribute);

                return IsValid = CanClose = false;
            }
        }

        return IsValid = CanClose = true;
    }

    public void FocusGroup(ConfigAttribute attribute)
    {
        SelectedGroup = Categories
            .Where(x => x.Header == attribute.Category).First().Groups
            .Where(x => x.Header == attribute.Group).First();
    }
}
