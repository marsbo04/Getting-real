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
