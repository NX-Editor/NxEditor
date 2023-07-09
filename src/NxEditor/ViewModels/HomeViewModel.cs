using NxEditor.Components;
using NxEditor.Core.Extensions;
using NxEditor.Views;

namespace NxEditor.ViewModels;

public class HomeViewModel : StaticPage<HomeViewModel, HomeView>
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
