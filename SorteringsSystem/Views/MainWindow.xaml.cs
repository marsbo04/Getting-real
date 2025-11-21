using System.Windows;
using System.ComponentModel;
using SorteringsSystem.ViewModels;


namespace SorteringsSystem.Views
{
    public partial class MainWindow : Window
    {
        private ViewTemplateSelector _selector;

        // Parameterless ctor kept for designer compatibility.
        public MainWindow() : this(new MainViewModel())
        {
        }

        // New ctor used by App to inject the MainViewModel (so the app-wide repository/controller is used).
        public MainWindow(MainViewModel vm)
        {
            InitializeComponent();

            InitializeWithViewModel(vm);
        }

        private void InitializeWithViewModel(MainViewModel vm)
        {
            DataContext = vm;

            _selector = (ViewTemplateSelector)Resources["TaskTemplateSelector"];
            vm.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(MainViewModel.IsListView))
                {
                    // Force the ItemsControl to refresh its containers so the selector is re-evaluated
                    _ = Dispatcher.InvokeAsync(() => TasksItemsControl?.Items.Refresh());
                }
            };
        }

        // Called from the CheckBox Checked/Unchecked events in XAML
        private void FilterCheckboxChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext is MainViewModel vm)
            {
                // Refresh the collection view so the Filter callback runs with updated dictionary values
                vm.FilteredTasks.Refresh();
            }
        }
        private void FilterComboBoxChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext is MainViewModel vm)
            {
                // Refresh the collection view so the Filter callback runs with updated dictionary values
                vm.FilteredTasks.Refresh();
            }
        }
    }
}