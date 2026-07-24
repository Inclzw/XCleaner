using Wpf.Ui.Abstractions.Controls;
using XCleaner.ViewModels.Pages;

namespace XCleaner.Views.Pages
{
    public partial class CleanerPage : INavigableView<CleanerViewModel>
    {
        public CleanerViewModel ViewModel { get; }

        public CleanerPage(CleanerViewModel viewModel)
        {
            ViewModel = viewModel;
            DataContext = this;

            InitializeComponent();
        }
    }
}