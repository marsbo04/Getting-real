using System.Windows;
using SorteringsSystem.ViewModels;

namespace SorteringsSystem.Views
{
    public partial class TaskDetailWindow : Window
    {
        public TaskDetailWindow()
        {
            InitializeComponent();
        }

        // Convenience ctor when VM is created externally (DialogService will wire close behavior)
        public TaskDetailWindow(TaskDetailViewModel vm) : this()
        {
            DataContext = vm;
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
