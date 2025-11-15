
using System.Collections.Generic;
using SorteringsSystem.Models;

namespace SorteringsSystem.ApplicationLayer
{
    public interface ITaskRepository
    {
        IEnumerable<TaskItem> GetAll();
        void Add(TaskItem task);
        void Update(TaskItem task);
        void Delete(TaskItem task);
    }
}