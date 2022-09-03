using System.Windows;
using System.Windows.Controls;
using Pose.Controls;

namespace Pose.Popups.ExportAnimations
{
    /// <summary>
    /// Interaction logic for ExportAnimationsView.xaml
    /// </summary>
    public partial class ExportAnimationsView : UserControl
    {
        public ExportAnimationsView()
        {
            InitializeComponent();
        }

        private void ExportAnimationsView_OnLoaded(object sender, RoutedEventArgs e)
        {
            ViewModel.Initialize();
        }

        public ExportAnimationsViewModel ViewModel => DataContext as ExportAnimationsViewModel;
    }
}
