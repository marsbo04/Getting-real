using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Data;
using System.Windows.Input;
using SorteringsSystem.Models;
using SorteringsSystem.ApplicationLayer;

namespace SorteringsSystem.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly TaskController _controller;

        public ObservableCollection<TaskItem> Tasks { get; set; }
        public ICollectionView FilteredTasks { get; set; }

        public ICommand OpenTaskCommand { get; }
        public ICommand ToggleViewCommand { get; }
        public ICommand CreateNewTaskCommand { get; }

        private bool _isListView;
        public bool IsListView
        {
            get => _isListView;
            set { _isListView = value; OnPropertyChanged(); }
        }

        public ObservableCollection<FilterOption> StatusFilters { get; set; }
        public ObservableCollection<FilterOption> PriorityFilters { get; set; }
        public ObservableCollection<FilterOption> ComplexityFilters { get; set; }

        // Default ctor for XAML / quick start — wires controller to in-memory repo.
        public MainViewModel() : this(new TaskController(new InMemoryTaskRepository())) { }

        // For DI/testing you can pass a TaskController with a different repository.
        public MainViewModel(TaskController controller)
        {
            _controller = controller;

            Tasks = new ObservableCollection<TaskItem>((IEnumerable<TaskItem>)_controller.GetTasks());

            StatusFilters = new ObservableCollection<FilterOption>
            {
                new FilterOption("Afvist"),
                new FilterOption("Under indtastning"),
                new FilterOption("Under arbejde"),
                new FilterOption("Afsluttet")
            };
            PriorityFilters = new ObservableCollection<FilterOption>
            {
                new FilterOption("Lav"),
                new FilterOption("Mellem"),
                new FilterOption("Høj")
            };
            ComplexityFilters = new ObservableCollection<FilterOption>
            {
                new FilterOption("Triviel"),
                new FilterOption("Simpel"),
                new FilterOption("Moderat"),
                new FilterOption("Kompleks"),
                new FilterOption("Kritisk")
            };

            HookFilterCollection(StatusFilters);
            HookFilterCollection(PriorityFilters);
            HookFilterCollection(ComplexityFilters);

            FilteredTasks = CollectionViewSource.GetDefaultView(Tasks);
            FilteredTasks.Filter = FilterTasks;

            OpenTaskCommand = new DelegateCommand<TaskItem>(OpenTask);
            ToggleViewCommand = new DelegateCommand(ToggleView);
            CreateNewTaskCommand = new DelegateCommand(CreateNewTask);
        }

        private void HookFilterCollection(ObservableCollection<FilterOption> collection)
        {
            collection.CollectionChanged += (s, e) =>
            {
                if (e.NewItems != null)
                {
                    foreach (FilterOption fo in e.NewItems) fo.PropertyChanged += FilterOption_PropertyChanged;
                }
                if (e.OldItems != null)
                {
                    foreach (FilterOption fo in e.OldItems) fo.PropertyChanged -= FilterOption_PropertyChanged;
                }
                FilteredTasks?.Refresh();
            };

            foreach (var fo in collection) fo.PropertyChanged += FilterOption_PropertyChanged;
        }

        private void FilterOption_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(FilterOption.IsSelected))
            {
                FilteredTasks?.Refresh();
            }
        }

        private bool FilterTasks(object obj)
        {
            if (obj is TaskItem task)
            {
                bool statusAny = StatusFilters.Any(f => f.IsSelected);
                bool statusMatch = !statusAny || StatusFilters.Any(f => f.IsSelected && task.Status == f.Name);

                bool priorityAny = PriorityFilters.Any(f => f.IsSelected);
                bool priorityMatch = !priorityAny || PriorityFilters.Any(f => f.IsSelected && task.Priority == f.Name);

                bool complexityAny = ComplexityFilters.Any(f => f.IsSelected);
                bool complexityMatch = !complexityAny || ComplexityFilters.Any(f => f.IsSelected && task.Complexity == f.Name);

                return statusMatch && priorityMatch && complexityMatch;
            }
            return false;
        }

        private void OpenTask(TaskItem task)
        {
            var vm = new TaskDetailViewModel(task);

            vm.SaveAction = t =>
            {
                _controller.SaveTask(t);
                if (!Tasks.Contains(t))
                {
                    Tasks.Add(t);
                }
            };

            vm.DeleteAction = t =>
            {
                _controller.DeleteTask(t);
                if (Tasks.Contains(t))
                {
                    Tasks.Remove(t);
                }
            };

            var window = new SorteringsSystem.Views.TaskDetailWindow(vm);

            void Handler(bool? r)
            {
                vm.RequestClose -= Handler;
                window.Dispatcher.BeginInvoke(new System.Action(() =>
                {
                    try
                    {
                        window.DialogResult = r;
                    }
                    catch (System.InvalidOperationException)
                    {
                        window.Close();
                    }
                }));
            }

            vm.RequestClose += Handler;
            window.ShowDialog();
        }

        private void ToggleView() => IsListView = !IsListView;

        private void CreateNewTask()
        {
            var newTask = new TaskItem
            {
                Title = "Ny opgave",
                Description = "Indtast beskrivelse...",
                Status = "Under indtastning",
                Priority = "Mellem",
                Complexity = "Simpel"
            };
            Tasks.Add(newTask);
            OpenTask(newTask);
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        private sealed class DelegateCommand : ICommand
        {
            private readonly System.Action _execute;
            private readonly System.Func<bool>? _canExecute;
            public DelegateCommand(System.Action execute, System.Func<bool>? canExecute = null)
            {
                _execute = execute;
                _canExecute = canExecute;
            }
            public bool CanExecute(object? parameter) => _canExecute == null || _canExecute();
            public void Execute(object? parameter) => _execute();
            public event System.EventHandler? CanExecuteChanged;
            public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, System.EventArgs.Empty);
        }

        private sealed class DelegateCommand<T> : ICommand
        {
            private readonly System.Action<T> _execute;
            private readonly System.Func<T, bool>? _canExecute;
            public DelegateCommand(System.Action<T> execute, System.Func<T, bool>? canExecute = null)
            {
                _execute = execute;
                _canExecute = canExecute;
            }
            public bool CanExecute(object? parameter) => _canExecute == null || _canExecute((T)parameter!);
            public void Execute(object? parameter) => _execute((T)parameter!);
            public event System.EventHandler? CanExecuteChanged;
            public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, System.EventArgs.Empty);
        }
    }
}
