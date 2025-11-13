using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace SorteringsSystem.Models
{
    public class FilterOption : INotifyPropertyChanged
    {
        public FilterOption(string name, bool isSelected = false)
        {
            Name = name;
            IsSelected = isSelected;
        }

        public string Name { get; }

        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set { _isSelected = value; OnPropertyChanged(); }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
    }
}