using Dock.Model.Mvvm.Controls;
using NxEditor.Core.Extensions;

namespace NxEditor.ViewModels;

public class HomeViewModel : Document
{
    public HomeViewModel()
    {
        Id = nameof(HomeViewModel);
        Title = "Home";
        CanFloat = false;
        CanClose = false;
        CanPin = false;
    }

    public static async Task VersionLink()
    {
        await BrowserExtension.OpenUrl("https://github.com/NX-Editor/NX-Editor/releases/latest");
    }
}
