using System.Windows;
using Pose.Controls;

namespace Pose.Popups.ExportAnimations
{
    /// <summary>
    /// Interaction logic for ExportAnimationsWindow.xaml
    /// </summary>
    public partial class ExportAnimationsWindow : ModernWindow
    {
        public ExportAnimationsWindow()
        {
            InitializeComponent();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void OK_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        public ExportAnimationsViewModel ViewModel => DataContext as ExportAnimationsViewModel;
    }
}
