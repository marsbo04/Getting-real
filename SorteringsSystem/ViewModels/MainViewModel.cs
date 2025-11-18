using SorteringsSystem.ApplicationLayer;
using SorteringsSystem.Models;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;

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
        public ObservableCollection<FilterOption> MailFilters { get; set; }

        private FilterOption _selectedMailFilter;
        public FilterOption SelectedMailFilter
        {
            get => _selectedMailFilter;
            set
            {
                _selectedMailFilter = value;
                OnPropertyChanged();
                FilteredTasks.Refresh();

            }
        }
        // Default ctor for XAML / quick start — wires controller to in-memory repo.
        public MainViewModel() : this(new TaskController(new InMemoryTaskRepository())) { }

        // For DI/testing you can pass a TaskController with a different repository.
        public MainViewModel(TaskController controller)
        {
            _controller = controller;

            Tasks = new ObservableCollection<TaskItem>((IEnumerable<TaskItem>)_controller.GetTasks());

            StatusFilters = new ObservableCollection<FilterOption>
            {
                new FilterOption("Under indtastning"),
                new FilterOption("Under arbejde"),
                new FilterOption("Afsluttet")
            };
            PriorityFilters = new ObservableCollection<FilterOption>
            {
                new FilterOption("Low"),
                new FilterOption("Medium"),
                new FilterOption("High")
            };
            ComplexityFilters = new ObservableCollection<FilterOption>
            {
                new FilterOption("Triviel"),
                new FilterOption("Simpel"),
                new FilterOption("Moderat"),
                new FilterOption("Kompleks"),
                new FilterOption("Kritisk")
            };

            MailFilters = new ObservableCollection<FilterOption>();
            AddMailFilter("Alle");
            foreach (var task in Tasks)
            {
                AddMailFilter(task.Mail);
            }
            

            HookFilterCollection(StatusFilters);
            HookFilterCollection(PriorityFilters);
            HookFilterCollection(ComplexityFilters);
            HookFilterCollection(MailFilters);

            FilteredTasks = CollectionViewSource.GetDefaultView(Tasks);
            FilteredTasks.Filter = FilterTasks;

            
            SelectedMailFilter = MailFilters.FirstOrDefault(f => f.Name == "Alle");

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

               

                bool mailMatch;
                if (SelectedMailFilter != null)
                {
                    if (SelectedMailFilter.Name == "Alle")
                        mailMatch = true;
                    else
                        mailMatch = task.Mail == SelectedMailFilter.Name;
                }
                else
                {
                    if (MailFilters.Count == 1 && MailFilters[0].Name == "Alle")
                    {
                        mailMatch = true;
                    }
                    else
                    {
                        bool mailAny = MailFilters.Any(f => f.IsSelected);
                        mailMatch = !mailAny || MailFilters.Any(f => f.IsSelected && task.Mail == f.Name);
                    }
                }

                return statusMatch && priorityMatch && complexityMatch && mailMatch;
            }
            return false;
        }

        private void AddMailFilter(string? mail)
        {
           
            if (string.IsNullOrWhiteSpace(mail)) return;

            if (!MailFilters.Any(m => m.Name == mail))
            {
                
                MailFilters.Add(new FilterOption(mail));
            }
        }

        private void OpenTask(TaskItem task)
        {
            var vm = new TaskDetailViewModel(task);
           

            vm.SaveAction = t =>
            {
                
                if (Tasks.Any(existing => !ReferenceEquals(existing, t)
                                         && !string.IsNullOrWhiteSpace(existing.Mail)
                                         && string.Equals(existing.Mail, t.Mail, StringComparison.OrdinalIgnoreCase)))
                {
                    
                    return;
                }

                AddMailFilter(t.Mail);

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
            var newTask = new TaskItem();         

            newTask.Title = "Ny opgave";
            newTask.Description = "Indtast beskrivelse...";
            newTask.Status = "Under indtastning";
            newTask.Mail = "eksempel@first.dk";
            

            Tasks.Add(newTask);
            OpenTask(newTask);
            newTask.ToString();
            

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
