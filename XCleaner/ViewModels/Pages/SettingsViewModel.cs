using System.IO;
using Velopack;
using Velopack.Sources;
using Wpf.Ui;
using Wpf.Ui.Abstractions.Controls;
using Wpf.Ui.Appearance;
using Wpf.Ui.Controls;
using Wpf.Ui.Extensions;

namespace XCleaner.ViewModels.Pages
{
    public partial class SettingsViewModel(IContentDialogService contentDialogService)
        : ObservableObject, INavigationAware
    {
        private bool _isInitialized;
        [ObservableProperty] private string _appVersion = string.Empty;
        [ObservableProperty] private ApplicationTheme _currentTheme = ApplicationTheme.Unknown;

        public Task OnNavigatedToAsync()
        {
            if (!_isInitialized)
            {
                InitializeViewModel();
            }

            return Task.CompletedTask;
        }

        public Task OnNavigatedFromAsync() => Task.CompletedTask;

        private void InitializeViewModel()
        {
            CurrentTheme = ApplicationThemeManager.GetAppTheme();
            AppVersion = $"版本信息: {GetAssemblyVersion()}";

            _isInitialized = true;
        }

        private static string GetAssemblyVersion()
        {
            var version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
            return version == null ? string.Empty : $"{version.Major}.{version.Minor}.{version.Build}";
        }

        [RelayCommand]
        private void OnChangeTheme(string parameter)
        {
            switch (parameter)
            {
                case "theme_light":
                    if (CurrentTheme == ApplicationTheme.Light)
                    {
                        break;
                    }

                    ApplicationThemeManager.Apply(ApplicationTheme.Light);
                    CurrentTheme = ApplicationTheme.Light;
                    break;
                default:
                    if (CurrentTheme == ApplicationTheme.Dark)
                    {
                        break;
                    }

                    ApplicationThemeManager.Apply(ApplicationTheme.Dark);
                    CurrentTheme = ApplicationTheme.Dark;
                    break;
            }
        }

        [RelayCommand]
        private async Task CheckForUpdates()
        {
#if DEBUG
            var path = Path.Combine(AppContext.BaseDirectory, @"..\..\..\..\Releases");
            var source = new SimpleFileSource(new DirectoryInfo(path));
            var updateManager = new UpdateManager(source, null, App.Locator);
#else
            var source = new GithubSource("https://github.com/Inclzw/XCleaner", accessToken: null, prerelease: false);
            var updateManager = new UpdateManager(source);
#endif
            var updateInfo = await updateManager.CheckForUpdatesAsync();
            if (updateInfo == null)
            {
                Console.WriteLine("No updates available.");
                return;
            }

            var result = await contentDialogService.ShowSimpleDialogAsync(new SimpleContentDialogCreateOptions
                {
                    Title = "已发现新版本，是否更新？",
                    Content = $"当前版本: {updateManager.CurrentVersion}, 最新版本: {updateInfo.TargetFullRelease.Version}",
                    PrimaryButtonText = "确定",
                    CloseButtonText = "取消"
                }
            );
            if (result == ContentDialogResult.Primary)
            {
                await updateManager.DownloadUpdatesAsync(updateInfo);
                updateManager.ApplyUpdatesAndRestart(updateInfo);
            }
        }
    }
}