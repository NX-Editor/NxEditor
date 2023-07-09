using Avalonia.Controls;
using ConfigFactory;
using ConfigFactory.Avalonia;
using ConfigFactory.Models;
using NxEditor.Core;

namespace NxEditor.Views;

public partial class ConfigView : UserControl
{
    public ConfigView()
    {
        InitializeComponent();
        if (ConfigPage.DataContext is ConfigPageModel configPage) {
            configPage.SecondaryButtonIsEnabled = false;
            configPage.Append<Config>();
        }
    }
}