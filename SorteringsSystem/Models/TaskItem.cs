using System.Collections.Generic;

namespace SorteringsSystem.Models
{
    public class TaskItem
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string Priority { get; set; } = string.Empty;
        public string Complexity { get; set; } = string.Empty;
        public List<SubTask> SubTasks { get; set; } = new();
        public string Note { get; set; } = string.Empty;
    }

    public class SubTask
    {
        public string Title { get; set; } = string.Empty;
        public string Text { get; set; } = string.Empty;
    }
}
