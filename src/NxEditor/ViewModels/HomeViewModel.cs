using Dock.Model.Mvvm.Controls;
using NxEditor.Components;
using NxEditor.Views;

namespace NxEditor.ViewModels;

public partial class HomeViewModel : Document
{
    public const string ID = "__home__";
    public const string DEFAULT_ICON = "fa-regular fa-copy";
    public const string DROPPING_ICON = "fa-regular fa-file-circle-plus";

    public HomeView View => ViewRepository.Get<HomeView>(this);

    [ObservableProperty]
    private string _icon = DEFAULT_ICON;

    public HomeViewModel()
    {
        Id = ID;
        Title = SystemMsg.Home;
        CanClose = false;
        CanPin = false;
        CanFloat = false;
    }
}
