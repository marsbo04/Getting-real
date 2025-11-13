using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using SorteringsSystem.Models;
using SorteringsSystem.Infrastructure;
using System.Threading.Tasks;

namespace SorteringsSystem.ViewModels
{
    public class TaskDetailViewModel : INotifyPropertyChanged, IDialogRequestClose
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
      
        // MVVM: view/service listens to this to close the dialog
        public event Action<bool?>? RequestClose;

        public TaskDetailViewModel(TaskItem task)
        {
            Task = task;
            SubTasks = new ObservableCollection<SubTask>(task.SubTasks);
            AddSubTaskCommand = new RelayCommand(AddSubTask);
            SaveTaskCommand = new RelayCommand(SaveTask);
            DeleteTaskCommand = new RelayCommand(DeleteTask);
        }

        private void AddSubTask()
        {
            var newSubTask = new SubTask { Title = "Ny underopgave", Text = "" };
            SubTasks.Add(newSubTask);
            Task.SubTasks.Add(newSubTask);
        }

        private void SaveTask()
        {
            // Let the caller persist/update the shared collection
            SaveAction?.Invoke(Task);

            // Signal to the view/service to close the dialog with a positive result
            RequestClose?.Invoke(true);

            MessageBox.Show("Opgaven er gemt!");
        }

        private void DeleteTask()
        {
            // Let the caller remove the task from the shared collection
            DeleteAction?.Invoke(Task);

            // Signal to the view/service to close the dialog with a negative/false result
            RequestClose?.Invoke(false);
            MessageBox.Show("Opgaven er slettet!");
        }

        public string Status { get => Task.Status; set { Task.Status = value; OnPropertyChanged(); } }
        public string Priority { get => Task.Priority; set { Task.Priority = value; OnPropertyChanged(); } }
        public string Complexity { get => Task.Complexity; set { Task.Complexity = value; OnPropertyChanged(); } }
        public string Note { get => Task.Note; set { Task.Note = value; OnPropertyChanged(); } }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}