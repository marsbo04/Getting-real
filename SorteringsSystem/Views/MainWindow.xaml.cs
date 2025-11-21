using System.Windows;
using System.ComponentModel;
using SorteringsSystem.ViewModels;


namespace SorteringsSystem.Views
{
    public partial class MainWindow : Window
    {
        private ViewTemplateSelector _selector;

      
        public MainWindow() : this(new MainViewModel())
        {
        }

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
                    
                    _ = Dispatcher.InvokeAsync(() => TasksItemsControl?.Items.Refresh());
                }
            };
        }

        
        private void FilterCheckboxChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext is MainViewModel vm)
            {
                
                vm.FilteredTasks.Refresh();
            }
        }
        private void FilterComboBoxChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext is MainViewModel vm)
            {
               
                vm.FilteredTasks.Refresh();
            }
        }
    }
}