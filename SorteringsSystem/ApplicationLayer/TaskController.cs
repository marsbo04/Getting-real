
using System.Collections.Generic;
using SorteringsSystem.ApplicationLayer;
using SorteringsSystem.Models;

namespace SorteringsSystem.ApplicationLayer
{
   
    public class TaskController
    {
        private readonly ITaskRepository _repository;

        public TaskController(ITaskRepository repository)
        {
            _repository = repository;
        }

        public IEnumerable<TaskItem> GetTasks() => _repository.GetAll();

        public void SaveTask(TaskItem task)
        {
            
            if (task == null) return;

           
            var exists = false;
            foreach (TaskItem tasktemp in _repository.GetAll())
            {
                if (ReferenceEquals(tasktemp, task)) { exists = true; break; }
            }

            if (exists)
                _repository.Update(task);
            else
                _repository.Add(task);
        }

        public void DeleteTask(TaskItem task)
        {
            if (task == null) return;
            _repository.Delete(task);
        }
    }
}