using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using SorteringsSystem.Models;
using System.Threading.Tasks;

namespace SorteringsSystem.ViewModels
{
    public class TaskDetailViewModel : INotifyPropertyChanged
    {
        private TaskItem _task;

        public TaskItem Task
        {
            get => _task;
            set { _task = value; OnPropertyChanged(); }
        }

        public ObservableCollection<SubTask> SubTasks { get; }
        public ICommand AddSubTaskCommand { get; }
        public ICommand SaveTaskCommand { get; }
        public ICommand DeleteTaskCommand { get; }

        // Callback the caller can set so Save will notify the caller (MainViewModel) to persist/update the Task collection
        public Action<TaskItem>? SaveAction { get; set; }

        public Action<TaskItem>? DeleteAction { get; set; }
      
        // MVVM: view listens to this to close the dialog (keeps same behavior; interface removed)
        public event Action<bool?>? RequestClose;

        public TaskDetailViewModel(TaskItem task)
        {
            Task = task;
            SubTasks = new ObservableCollection<SubTask>(task.SubTasks);
            AddSubTaskCommand = new DelegateCommand(AddSubTask);
            SaveTaskCommand = new DelegateCommand(SaveTask);
            DeleteTaskCommand = new DelegateCommand(DeleteTask);
        }

        private void AddSubTask()
        {
            var newSubTask = new SubTask { Title = "Ny underopgave", Text = "" };
            SubTasks.Add(newSubTask);
            Task.SubTasks.Add(newSubTask);
        }

        private void SaveTask()
        {
            SaveAction?.Invoke(Task);
            RequestClose?.Invoke(true);
            MessageBox.Show("Opgaven er gemt!");
        }

        private void DeleteTask()
        {
            DeleteAction?.Invoke(Task);
            RequestClose?.Invoke(false);
            MessageBox.Show("Opgaven er slettet!");
        }

        public string Status { get => Task.Status; set { Task.Status = value; OnPropertyChanged(); } }
        public string Priority { get => Task.Priority; set { Task.Priority = value; OnPropertyChanged(); } }
        public string Complexity { get => Task.Complexity; set { Task.Complexity = value; OnPropertyChanged(); } }
        public string Note { get => Task.Note; set { Task.Note = value; OnPropertyChanged(); } }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        // Small local ICommand implementation (replacement for RelayCommand)
        private sealed class DelegateCommand : ICommand
        {
            private readonly Action _execute;
            private readonly Func<bool>? _canExecute;
            public DelegateCommand(Action execute, Func<bool>? canExecute = null)
            {
                _execute = execute;
                _canExecute = canExecute;
            }
            public bool CanExecute(object? parameter) => _canExecute == null || _canExecute();
            public void Execute(object? parameter) => _execute();
            public event EventHandler? CanExecuteChanged;
            public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}