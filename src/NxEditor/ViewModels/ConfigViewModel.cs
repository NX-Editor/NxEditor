using ConfigFactory.Models;

namespace NxEditor.ViewModels;

public class ConfigViewModel : ConfigPageModel, IStaticPage<ConfigViewModel, ConfigView>
{
    public ConfigView View { get; }

    ConfigViewModel IStaticPage<ConfigViewModel, ConfigView>.Shared => Shared;
    public static ConfigViewModel Shared { get; } = new();

    public ConfigViewModel()
    {
        Id = nameof(ConfigViewModel);
        Title = "Settings";
        CanFloat = false;

        View = new() {
            DataContext = this
        };
    }
}
