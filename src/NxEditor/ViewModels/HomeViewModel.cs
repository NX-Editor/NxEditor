using Dock.Model.Mvvm.Controls;
using NxEditor.Components;
using NxEditor.Views;

namespace NxEditor.ViewModels;

public class HomeViewModel : Document
{
    public const string ID = "__home__";

    public HomeView View => ViewRepository.Get<HomeView>(this);

    public HomeViewModel()
    {
        Title = SystemMsg.Home;
        Id = ID;
    }
}
