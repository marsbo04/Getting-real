using SorteringsSystem.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;
using System.Text.RegularExpressions;
using System.Diagnostics;

namespace SorteringsSystem.ApplicationLayer
{
    public class InMemoryTaskRepository : ITaskRepository
    {
       
        private static readonly string[] KnownTextFolders = { "TextFiles", "TexstFiles" };

       
        private string path;
        private readonly List<TaskItem> _store = new();

       
        public InMemoryTaskRepository(string? initialPath = null)
        {
            
            if (string.IsNullOrWhiteSpace(initialPath))
            {
                this.path = ResolveTextFilesTasksPath();
            }
            else
            {
                
                var fileName = Path.GetFileName(initialPath);
                var dirName = Path.GetDirectoryName(initialPath);

                if (string.Equals(fileName, "Tasks.txt", StringComparison.OrdinalIgnoreCase)
                    && (string.IsNullOrEmpty(dirName) || dirName == "." || !Path.IsPathRooted(initialPath)))
                {
                    this.path = ResolveTextFilesTasksPath();
                }
                else
                {
                    
                    var full = Path.GetFullPath(initialPath);

                    if (PathContainsKnownTextFolder(full))
                    {
                        
                        this.path = ResolveTextFilesTasksPath() ?? full;
                    }
                    else
                    {
                        this.path = full;
                    }
                }
            }

            Debug.WriteLine($"Using tasks file: {this.path}");

            EnsureFileExists();
            LoadTaskFile();
        }

        public static string? ResolveTextFilesTasksPath()
        {
            
            var starts = new[] { AppContext.BaseDirectory, Directory.GetCurrentDirectory() };

            foreach (var start in starts)
            {
                try
                {
                    var dir = new DirectoryInfo(start);
                    while (dir != null)
                    {
                        foreach (var folderName in KnownTextFolders)
                        {
                            var candidateFolder = Path.Combine(dir.FullName, folderName);
                            if (Directory.Exists(candidateFolder))
                            {
                                return Path.GetFullPath(Path.Combine(candidateFolder, "Tasks.txt"));
                            }
                        }

                        dir = dir.Parent;
                    }
                }
                catch {  }
            }

            
            return Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, KnownTextFolders[0], "Tasks.txt"));
        }

        private static bool PathContainsKnownTextFolder(string fullPath)
        {
            if (string.IsNullOrEmpty(fullPath)) return false;

            foreach (var name in KnownTextFolders)
            {
                if (fullPath.IndexOf(Path.DirectorySeparatorChar + name + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase) >= 0
                    || fullPath.IndexOf(Path.AltDirectorySeparatorChar + name + Path.AltDirectorySeparatorChar, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return true;
                }
            }

            return false;
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

        private void EnsureFileExists()
        {
            var dir = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            if (!File.Exists(path))
                File.Create(path).Dispose();
        }

        
        public void LoadTaskFile()
        {
            _store.Clear();

            if (!File.Exists(path))
                return;

            using StreamReader streamReader = new StreamReader(path);
            int lineNo = 0;
            while (!streamReader.EndOfStream)
            {
                string? line = streamReader.ReadLine();
                lineNo++;

                if (string.IsNullOrWhiteSpace(line))
                    continue;

                line = line.Trim();
                if (line.Length > 0 && line[0] == '\uFEFF')
                    line = line.Substring(1);

                try
                {
                    TaskItem task = ParseLine(line);
                    _store.Add(task);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Failed to parse task file line {lineNo}: {ex.Message}. Attempting fallback parse.");

                    try
                    {
                        var fallback = FallbackParseLine(line);
                        _store.Add(fallback);
                    }
                    catch (Exception inner)
                    {
                        Debug.WriteLine($"Fallback parse failed for line {lineNo}: {inner.Message}");
                    }
                }
            }
        }

        private TaskItem FallbackParseLine(string line)
        {
            var task = new TaskItem();

            var itemPattern = new Regex(@"\[[^\]]*\]");
            var brackets = itemPattern.Matches(line).Cast<Match>().Select(m => m.Value).ToList();

            string lineWithoutBrackets = itemPattern.Replace(line, "__BRACKETS__");

            var parts = lineWithoutBrackets.Split(new[] { ", " }, StringSplitOptions.None);

            int bracketIndex = 0;
            foreach (var part in parts)
            {
                if (string.IsNullOrWhiteSpace(part))
                    continue;

                if (part.StartsWith("SubTasks:"))
                {
                    string suffix = part.Substring("SubTasks:".Length).Trim();
                    string reconstructed = "";

                    for (int i = bracketIndex; i < brackets.Count; i++)
                    {
                        if (reconstructed.Length > 0) reconstructed += " ";
                        reconstructed += brackets[i];
                    }
                    var value = reconstructed == "" ? suffix : reconstructed;

                    var itemMatches = new Regex(@"\[(?<content>[^\]]+)\]").Matches(value);
                    foreach (Match im in itemMatches)
                    {
                        string content = im.Groups["content"].Value.Trim();
                        content = Regex.Replace(content, @"^\d+\s+", "");
                        var subMatch = Regex.Match(content, @"Title: (?<title>""(?:\\.|[^""])*""|[^,]+),\s*Text: (?<text>""(?:\\.|[^""])*""|.+)$", RegexOptions.Singleline);
                        if (subMatch.Success)
                        {
                            var title = UnquoteIfQuoted(subMatch.Groups["title"].Value.Trim());
                            var text = UnquoteIfQuoted(subMatch.Groups["text"].Value.Trim());
                            task.SubTasks.Add(new SubTask { Title = title, Text = text });
                        }
                        else
                        {
                            task.SubTasks.Add(new SubTask { Title = string.Empty, Text = UnquoteIfQuoted(content) });
                        }
                    }
                    break;
                }
                else
                {
                    var kv = part.Split(new[] { ": " }, 2, StringSplitOptions.None);
                    if (kv.Length < 2)
                        continue;
                    string key = kv[0].Trim();
                    string value = kv[1].Trim();
                    switch (key)
                    {
                        case "Title":
                            task.Title = UnquoteIfQuoted(value);
                            break;
                        case "Description":
                            task.Description = UnquoteIfQuoted(value);
                            break;
                        case "Mail":
                            task.Mail = UnquoteIfQuoted(value);
                            break;
                        case "Status":
                            task.Status = UnquoteIfQuoted(value);
                            break;
                        case "Priority":
                            task.Priority = UnquoteIfQuoted(value);
                            break;
                        case "Complexity":
                            task.Complexity = UnquoteIfQuoted(value);
                            break;
                        case "Note":
                            task.Note = UnquoteIfQuoted(value);
                            break;
                    }
                }

                if (part.Contains("__BRACKETS__") && bracketIndex < brackets.Count)
                    bracketIndex++;
            }

            return task;
        }

        private TaskItem ParseLine(string line)
        {
            var task = new TaskItem();

            
            var pairPattern = new Regex(@"(?<key>\w+): (?<value>(?:\[[^\]]*\](?:\s*\[[^\]]*\])*)|[^,]*)(?:, |$)", RegexOptions.Singleline);
            var matches = pairPattern.Matches(line);

            foreach (Match m in matches)
            {
                string key = m.Groups["key"].Value;
                string value = m.Groups["value"].Value.Trim();

                switch (key)
                {
                    case "Title":
                        task.Title = UnquoteIfQuoted(value);
                        break;
                    case "Description":
                        task.Description = UnquoteIfQuoted(value);
                        break;
                    case "Mail":
                        task.Mail = UnquoteIfQuoted(value);
                        break;
                    case "Status":
                        task.Status = UnquoteIfQuoted(value);
                        break;
                    case "Priority":
                        task.Priority = UnquoteIfQuoted(value);
                        break;
                    case "Complexity":
                        task.Complexity = UnquoteIfQuoted(value);
                        break;
                    case "Note":
                        task.Note = UnquoteIfQuoted(value);
                        break;
                    case "SubTasks":
                        var itemPattern = new Regex(@"\[(?<content>[^\]]+)\]");
                        var items = itemPattern.Matches(value);
                        foreach (Match im in items)
                        {
                            string content = im.Groups["content"].Value.Trim();
                            content = Regex.Replace(content, @"^\d+\s+", "");

                            var subMatch = Regex.Match(content,
                                @"Title: (?<title>""(?:\\.|[^""])*""|[^,]+),\s*Text: (?<text>""(?:\\.|[^""])*""|.+)$",
                                RegexOptions.Singleline);
                            if (subMatch.Success)
                            {
                                var titleRaw = subMatch.Groups["title"].Value.Trim();
                                var textRaw = subMatch.Groups["text"].Value.Trim();
                                var title = UnquoteIfQuoted(titleRaw);
                                var text = UnquoteIfQuoted(textRaw);
                                task.SubTasks.Add(new SubTask { Title = title, Text = text });
                            }
                            else
                            {
                                task.SubTasks.Add(new SubTask { Title = string.Empty, Text = UnquoteIfQuoted(content) });
                            }
                        }
                        break;
                }
            }

            return task;
        }

        private void UpdateTaskFile()
        {
            using StreamWriter streamWriter = new StreamWriter(path, false);
            foreach (TaskItem item in _store)
            {
                streamWriter.WriteLine(SerializeTask(item));
            }
        }

        private static string SerializeTask(TaskItem item)
        {
            string header = $"Title: {QuoteIfNeeded(item.Title)}, Description: {QuoteIfNeeded(item.Description)}, Mail: {QuoteIfNeeded(item.Mail)}, Status: {QuoteIfNeeded(item.Status)}, Priority: {QuoteIfNeeded(item.Priority)}, Complexity: {QuoteIfNeeded(item.Complexity)}, Note: {QuoteIfNeeded(item.Note)}, SubTasks: ";
            if (item.SubTasks == null || item.SubTasks.Count == 0)
                return header + "[]";

            var parts = new List<string>();
            for (int i = 0; i < item.SubTasks.Count; i++)
            {
                var s = item.SubTasks[i];
                var titleQuoted = QuoteIfNeeded(s.Title);
                var textQuoted = QuoteIfNeeded(s.Text);
                parts.Add($"[{i} Title: {titleQuoted}, Text: {textQuoted}]");
            }

            return header + string.Join(" ", parts);
        }

        private static string QuoteIfNeeded(string? input)
        {
            input ??= string.Empty;
            bool needsQuotes = input.Contains(',') || input.Contains(':') || input.Contains(']') || input.Contains('[') || input.Contains('"') || input.Contains('\\') || input.Contains('\n') || input.Contains('\r') || string.IsNullOrWhiteSpace(input);
            if (!needsQuotes) return input;
            var escaped = input.Replace("\\", "\\\\").Replace("\"", "\\\"");
            return $"\"{escaped}\"";
        }

        private static string UnquoteIfQuoted(string input)
        {
            input = input?.Trim() ?? string.Empty;
            if (input.Length >= 2 && input[0] == '"' && input[^1] == '"')
            {
                var inner = input.Substring(1, input.Length - 2);

                return inner.Replace("\\\"", "\"").Replace("\\\\", "\\");
            }
            return input;
        }
    }
}