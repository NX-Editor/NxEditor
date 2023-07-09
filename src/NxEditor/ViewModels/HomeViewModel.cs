using CommunityToolkit.Mvvm.Input;
using NxEditor.Components;
using NxEditor.Core.Extensions;
using NxEditor.Views;

namespace NxEditor.ViewModels;

public partial class HomeViewModel : StaticPage<HomeViewModel, HomeView>
{
    public HomeViewModel()
    {
        Id = nameof(HomeViewModel);
        Title = "Home";
        CanFloat = false;
        CanClose = false;
        CanPin = false;
    }

    [RelayCommand]
    public static async Task VersionLink()
    {
        await BrowserExtension.OpenUrl("https://github.com/NX-Editor/NxEditor/releases/latest");
    }
}
