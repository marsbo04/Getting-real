
using System.Collections.Generic;
using System.Linq;
using SorteringsSystem.Models;

namespace SorteringsSystem.ApplicationLayer
{
    // Simple in-memory repository used for prototyping and unit tests.
    public class InMemoryTaskRepository : ITaskRepository
    {
        private readonly List<TaskItem> _store = new();

        public InMemoryTaskRepository()
        {
            // Seed data (keeps same sample data as before)
            _store.Add(new TaskItem
            {
                Title = "Bestil ny mobil telefon",
                Description = "Vi skal bestille en ny Samsung Galaxy til medarbejderen",
                Mail = "Test1@Test.dk",
                Status = "Under arbejde",
                Priority = "High",                
                Complexity = "Simpel"
            });
            _store.Add(new TaskItem
            {
                Title = "Opdater firmawebsite",
                Description = "Websitet skal have nye produktbilleder og opdateret indhold",
                Mail = "Test2@Test.dk",
                Status = "Under indtastning",
                Priority = "Medium",
                Complexity = "Moderat"
            });
        }

        public IEnumerable<TaskItem> GetAll() => _store;

        public void Add(TaskItem task)
        {
            if (!_store.Contains(task))
            {
                task.ToString();
                _store.Add(task);
            }
        }

        public void Update(TaskItem task)
        {
            // For the in-memory implementation we keep reference semantics:
            // the TaskItem instance already in the list will reflect changes.
            // For real persistence, map and save changes here.
            if (!_store.Contains(task))
            {
                task.ToString();
                
                _store.Add(task);
            }
        }

        public void Delete(TaskItem task)
        {
            if (_store.Contains(task))
                _store.Remove(task);
        }
    }
}