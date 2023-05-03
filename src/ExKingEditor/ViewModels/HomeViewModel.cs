using Dock.Model.Mvvm.Controls;
using ExKingEditor.Core.Extensions;

namespace ExKingEditor.ViewModels;

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
        await BrowserExtension.OpenUrl("https://github.com/EXKing-Editor/EXKing-Editor/releases/latest");
    }
}
