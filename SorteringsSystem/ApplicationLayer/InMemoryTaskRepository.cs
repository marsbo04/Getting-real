using SorteringsSystem.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SorteringsSystem.ApplicationLayer
{
    // Simple in-memory repository used for prototyping and unit tests.
    public class InMemoryTaskRepository : ITaskRepository
    {
        private readonly List<TaskItem> _store = new();

        public InMemoryTaskRepository()
        {
            Load();
            // Seed data (keeps same sample data as before)
            _store.Add(new TaskItem
            {
                Title = "Bestil ny mobil telefon",
                Description = "Vi skal bestille en ny Samsung Galaxy til medarbejderen",
                Mail = "Test1@Test.dk",
                Status = "Under arbejde",
                Priority = "Høj",
                Complexity = "Simpel"
            });
            _store.Add(new TaskItem
            {
                Title = "Opdater firmawebsite",
                Description = "Websitet skal have nye produktbilleder og opdateret indhold",
                Mail = "Test2@Test.dk",
                Status = "Under indtastning",
                Priority = "Mellem",
                Complexity = "Moderat"
            });
        }

        public IEnumerable<TaskItem> GetAll() => _store;

        public void Add(TaskItem task)
        {
            if (task == null) return;

            if (!_store.Contains(task))
            {
                task.ToString();
                _store.Add(task);

                StreamWriter streamWriter = new StreamWriter("C:\\Users\\nickl\\source\\repos\\marsbo04\\Getting-real\\SorteringsSystem\\Tasks.txt");
                using (streamWriter)
                {
                    foreach (TaskItem item in _store)
                    {
                        streamWriter.WriteLine(item.ToString());
                    }
                }
            }
        }

        public void Update(TaskItem task)
        {
            if (task == null) return;


            if (_store.Contains(task)) return;


            if (_store.Any(t => !string.IsNullOrWhiteSpace(t.Mail) && string.Equals(t.Mail, task.Mail, StringComparison.OrdinalIgnoreCase)))
                return;

            task.ToString();
            _store.Add(task);
        }

        public void Delete(TaskItem task)
        {
            if (_store.Contains(task))
                _store.Remove(task);
        }
        public void Load()
        {
            string path = "C:\\Users\\nickl\\source\\repos\\marsbo04\\Getting-real\\SorteringsSystem\\Tasks.txt";
            string line = "";
            using StreamReader sr = new StreamReader(path);
            {
                while (line != null)
                    line = sr.ReadLine();
            }
        }
    }
}