using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using SorteringsSystem.Models;
using System.Linq;
using System.Collections.Generic;

namespace SorteringsSystem.ViewModels
{
    public class TaskDetailViewModel : INotifyPropertyChanged
    {
        private readonly TaskItem _originalTask;
        private TaskItem _task = null!;

        public TaskItem Task
        {
            get => _task;
            set { _task = value; OnPropertyChanged(); }
        }

        public ObservableCollection<SubTask> SubTasks { get; }
        public ICommand AddSubTaskCommand { get; }
        public ICommand SaveTaskCommand { get; }
        public ICommand DeleteTaskCommand { get; }

        
        public Action<TaskItem>? SaveAction { get; set; }

        public Action<TaskItem>? DeleteAction { get; set; }
      
        
        public event Action<bool?>? RequestClose;

        public TaskDetailViewModel(TaskItem task)
        {
            _originalTask = task ?? throw new ArgumentNullException(nameof(task));
            Task = CloneTask(_originalTask);

            SubTasks = new ObservableCollection<SubTask>(Task.SubTasks ?? new ObservableCollection<SubTask>());
            AddSubTaskCommand = new DelegateCommand(AddSubTask);
            SaveTaskCommand = new DelegateCommand(SaveTask);
            DeleteTaskCommand = new DelegateCommand(DeleteTask);
        }

        private TaskItem CloneTask(TaskItem t)
        {
            return new TaskItem
            {
                Title = t.Title,
                Description = t.Description,
                Mail = t.Mail,
                Status = t.Status,
                Priority = t.Priority,
                Complexity = t.Complexity,
                Note = t.Note,
                SubTasks = new ObservableCollection<SubTask>((t.SubTasks ?? new ObservableCollection<SubTask>()).Select(st => new SubTask { Title = st.Title, Text = st.Text }))
            };
        }

        private void AddSubTask()
        {
            SubTask newSubTask = new SubTask { Title = "Ny underopgave", Text = "" };
            SubTasks.Add(newSubTask);
            Task.SubTasks ??= new ObservableCollection<SubTask>();
            Task.SubTasks.Add(newSubTask);
        }

        private void SaveTask()
        {
            _originalTask.Title = Task.Title;
            _originalTask.Description = Task.Description;
            _originalTask.Mail = Task.Mail;
            _originalTask.Status = Task.Status;
            _originalTask.Priority = Task.Priority;
            _originalTask.Complexity = Task.Complexity;
            _originalTask.Note = Task.Note;
            _originalTask.SubTasks = new ObservableCollection<SubTask>(SubTasks.Select(s => new SubTask { Title = s.Title, Text = s.Text }));

            SaveAction?.Invoke(_originalTask);
            RequestClose?.Invoke(true);
            
            MessageBox.Show("Opgaven er gemt!");
        }

        private void DeleteTask()
        {
            DeleteAction?.Invoke(_originalTask);
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