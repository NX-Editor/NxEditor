using ConfigFactory;
using ConfigFactory.Models;
using NxEditor.Core;

namespace NxEditor.ViewModels;

public class ConfigViewModel : ConfigPageModel
{
    public ConfigViewModel()
    {
        Id = nameof(ConfigViewModel);
        Title = "Settings";
        CanFloat = false;
    }
}
