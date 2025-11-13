using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Data;
using System.Windows.Input;
using System.Collections.Specialized;
using SorteringsSystem.Models;
using SorteringsSystem.Infrastructure;

namespace SorteringsSystem.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<TaskItem> Tasks { get; set; } = new ObservableCollection<TaskItem>();
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

        // Replaced dictionaries with observable collections of FilterOption (pure MVVM)
        public ObservableCollection<FilterOption> StatusFilters { get; set; }
        public ObservableCollection<FilterOption> PriorityFilters { get; set; }
        public ObservableCollection<FilterOption> ComplexityFilters { get; set; }

        public MainViewModel()
        {
            Tasks.Add(new TaskItem { Title = "Bestil ny mobil telefon", Description = "Vi skal bestille en ny Samsung Galaxy til medarbejderen", Status = "Under arbejde", Priority = "Høj", Complexity = "Simpel" });
            Tasks.Add(new TaskItem { Title = "Opdater firmawebsite", Description = "Websitet skal have nye produktbilleder og opdateret indhold", Status = "Under indtastning", Priority = "Mellem", Complexity = "Moderat" });

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

            // Wire collection change and item property change to refresh the view when filters change
            HookFilterCollection(StatusFilters);
            HookFilterCollection(PriorityFilters);
            HookFilterCollection(ComplexityFilters);

            FilteredTasks = CollectionViewSource.GetDefaultView(Tasks);
            FilteredTasks.Filter = FilterTasks;

            OpenTaskCommand = new RelayCommand<TaskItem>(OpenTask);
            ToggleViewCommand = new RelayCommand(ToggleView);
            CreateNewTaskCommand = new RelayCommand(CreateNewTask);
        }

        private void HookFilterCollection(ObservableCollection<FilterOption> collection)
        {
            collection.CollectionChanged += (s, e) =>
            {
                // subscribe new items, unsubscribe removed
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
                // Re-evaluate CollectionView filter on changes to any filter option
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
            // Create VM and let the caller (MainViewModel) update the shared Tasks collection when Save is triggered
            var vm = new TaskDetailViewModel(task);
            vm.SaveAction = t =>
            {
                if (!Tasks.Contains(t))
                {
                    Tasks.Add(t);
                }
            };

            // Ensure DeleteAction removes the task from the shared Tasks collection.
            // This ensures pressing Delete removes the item and does not trigger SaveAction.
            vm.DeleteAction = t =>
            {
                if (Tasks.Contains(t))
                {
                    Tasks.Remove(t);
                }
            };

            var window = new SorteringsSystem.Views.TaskDetailWindow(vm);
            var dialogService = new SorteringsSystem.Services.DialogService();
            var result = dialogService.ShowDialog(window, vm);

            // result == true => saved; result == false => deleted; null => closed without a decision
            // (SaveAction already updated Tasks when Save was invoked; DeleteAction removes the task when Delete is invoked)
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
    }
}
