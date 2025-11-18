using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace SorteringsSystem.Models
{
    public class TaskItem : INotifyPropertyChanged
    {
        private string _title = string.Empty;
        public string Title { get => _title; set { if (_title == value) return; _title = value; OnPropertyChanged(); } }

        private string _description = string.Empty;
        public string Description { get => _description; set { if (_description == value) return; _description = value; OnPropertyChanged(); } }

        private string _mail = string.Empty;
        public string Mail { get => _mail; set { if (_mail == value) return; _mail = value; OnPropertyChanged(); } }

        private string _status = string.Empty;
        public string Status { get => _status; set { if (_status == value) return; _status = value; OnPropertyChanged(); } }

        private string _priority = string.Empty;
        public string Priority { get => _priority; set { if (_priority == value) return; _priority = value; OnPropertyChanged(); } }

        private string? _complexity = string.Empty;
        public string? Complexity { get => _complexity; set { if (_complexity == value) return; _complexity = value; OnPropertyChanged(); } }

        private ObservableCollection<SubTask> _subTasks = new();
        public ObservableCollection<SubTask> SubTasks { get => _subTasks; set { if (_subTasks == value) return; _subTasks = value; OnPropertyChanged(); } }

        private string? _note = string.Empty;
        public string? Note { get => _note; set { if (_note == value) return; _note = value; OnPropertyChanged(); } }

        public override string ToString()
        {
            return $"Title: {Title}, Description: {Description}, Mail: {Mail}, Status: {Status}, Priority: {Priority}, Complexity: {Complexity}, Note: {Note}, SubTasks: {SubTasks?.Count ?? 0}";
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public class SubTask : INotifyPropertyChanged
    {
        private string _title = string.Empty;
        public string Title { get => _title; set { if (_title == value) return; _title = value; OnPropertyChanged(); } }

        private string _text = string.Empty;
        public string Text { get => _text; set { if (_text == value) return; _text = value; OnPropertyChanged(); } }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
