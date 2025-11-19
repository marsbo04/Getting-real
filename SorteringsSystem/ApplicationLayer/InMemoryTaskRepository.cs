using SorteringsSystem.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;

namespace SorteringsSystem.ApplicationLayer
{
    // Simple in-memory repository used for prototyping and unit tests.
    public class InMemoryTaskRepository : ITaskRepository
    {
        private readonly List<TaskItem> _store = new();
        private readonly string path = "C:\\Users\\nickl\\source\\repos\\marsbo04\\Getting-real\\SorteringsSystem\\Tasks.txt";

        public InMemoryTaskRepository()
        {
            LoadTaskFile();
        }

        public IEnumerable<TaskItem> GetAll() => _store;

        public void Add(TaskItem task)
        {
            if (task == null) return;

            if (!_store.Contains(task))
            {
                task.ToString();
                _store.Add(task);
                UpdateTaskFile();
            }
        }

        public void Update(TaskItem task)
        {
            if (task == null) return;

            if (_store.Contains(task))
                UpdateTaskFile();
        }

        public void Delete(TaskItem task)
        {
            if (_store.Contains(task))
            {
                _store.Remove(task);
                UpdateTaskFile();
            }
        }
        public void LoadTaskFile()
        {
            using StreamReader streamReader = new StreamReader(path);
            {
                while (!streamReader.EndOfStream)
                {
                    TaskItem task = new TaskItem();

                    string line = streamReader.ReadLine();

                    string[] parts = line.Split(", ");

                    foreach (string part in parts)
                    {
                        string[] keyValue = part.Split(": ", 2);
                        
                        if (keyValue.Length < 2)
                            break;

                        string key = keyValue[0];
                        string value = keyValue[1];

                        switch (key)
                        {
                            case "Title":
                                task.Title = value;
                                break;
                            case "Description":
                                task.Description = value;
                                break;
                            case "Mail":
                                task.Mail = value;
                                break;
                            case "Status":
                                task.Status = value;
                                break;
                            case "Priority":
                                task.Priority = value;
                                break;
                            case "Complexity":
                                task.Complexity = value;
                                break;
                            case "Note":
                                task.Note = value;
                                break;
                            case "SubTasks":
                                int number;
                                int.TryParse(value, out number);
                                break;
                        }
                    }
                        _store.Add(task);
                }
            }
        }
        public void UpdateTaskFile()
        {
            StreamWriter streamWriter = new StreamWriter(path);
            using (streamWriter)
            {
                foreach (TaskItem item in _store)
                {
                    streamWriter.WriteLine(item);
                }
            }
        }
    }
}