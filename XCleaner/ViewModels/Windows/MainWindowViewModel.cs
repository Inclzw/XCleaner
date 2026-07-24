using System.Collections.ObjectModel;
using Wpf.Ui.Controls;

namespace XCleaner.ViewModels.Windows
{
    public partial class MainWindowViewModel : ObservableObject
    {
        [ObservableProperty] private string _applicationTitle = "XCleaner";

        [ObservableProperty] private ObservableCollection<object> _menuItems =
        [
            // new NavigationViewItem
            // {
            //     Content = "Home",
            //     Icon = new SymbolIcon { Symbol = SymbolRegular.Home24 },
            //     TargetPageType = typeof(Views.Pages.DashboardPage)
            // },
            // new NavigationViewItem
            // {
            //     Content = "Data",
            //     Icon = new SymbolIcon { Symbol = SymbolRegular.DataHistogram24 },
            //     TargetPageType = typeof(Views.Pages.DataPage)
            // },
            new NavigationViewItem
            {
                Content = "Cleaner",
                Icon = new SymbolIcon { Symbol = SymbolRegular.Broom24 },
                TargetPageType = typeof(Views.Pages.CleanerPage)
            }
        ];

        [ObservableProperty] private ObservableCollection<object> _footerMenuItems =
        [
            new NavigationViewItem
            {
                Content = "Settings",
                Icon = new SymbolIcon { Symbol = SymbolRegular.Settings24 },
                TargetPageType = typeof(Views.Pages.SettingsPage)
            }
        ];

        [ObservableProperty] private ObservableCollection<MenuItem> _trayMenuItems =
            [new() { Header = "Home", Tag = "tray_home" }];
    }
}