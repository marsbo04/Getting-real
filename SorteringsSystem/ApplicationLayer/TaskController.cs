
using System.Collections.Generic;
using SorteringsSystem.ApplicationLayer;
using SorteringsSystem.Models;

namespace SorteringsSystem.ApplicationLayer
{
    // Controller coordinates use-cases (application logic) and talks to repository.
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
            // Basic "upsert" behavior. Add domain validations here if needed.
            if (task == null) return;

            // Because TaskItem has no stable Id, we detect by reference membership.
            // Consider adding an Id property for robust identity.
            var exists = false;
            foreach (var t in _repository.GetAll())
            {
                if (ReferenceEquals(t, task)) { exists = true; break; }
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