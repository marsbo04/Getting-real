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
        //known folders to search for the Tasks.txt file
        private static readonly string[] KnownTextFolders = { "TextFiles", "TexstFiles" };

        //path to the tasks file
        private string path;
        private readonly List<TaskItem> _store = new();

        //constructor that accepts an optional path to the tasks file
        public InMemoryTaskRepository(string? initialPath = null)
        {
            //determine the path to use
            if (string.IsNullOrWhiteSpace(initialPath))
            {
                //no path provided, try to resolve it
                this.path = ResolveTextFilesTasksPath();
            }
            else
            {
                //path provided, check if it's just "Tasks.txt" or contains known folder names
                var fileName = Path.GetFileName(initialPath);
                //get directory name
                var dirName = Path.GetDirectoryName(initialPath);
                //check conditions
                if (string.Equals(fileName, "Tasks.txt", StringComparison.OrdinalIgnoreCase)
                    // || dirName == null
                    && (string.IsNullOrEmpty(dirName) || dirName == "." || !Path.IsPathRooted(initialPath)))
                {
                    //just "Tasks.txt" or relative path, try to resolve
                    this.path = ResolveTextFilesTasksPath();
                }
                //path contains known folder names
                else
                {
                    //get full path
                    var full = Path.GetFullPath(initialPath);
                    //check if it contains known text folder names
                    if (PathContainsKnownTextFolder(full))
                    {
                        //try to resolve to preferred location
                        this.path = ResolveTextFilesTasksPath() ?? full;
                    }
                    else
                    {
                        //use the provided full path
                        this.path = full;
                    }
                }
            }
            //log the path being used
            Debug.WriteLine($"Using tasks file: {this.path}");
            //ensure the file exists and load tasks
            EnsureFileExists();
            //load tasks from file
            LoadTaskFile();
        }
        //this helper to find the Tasks.txt file in known folders to make it easier to locate
        public static string? ResolveTextFilesTasksPath()
        {
            //search from base directory and current directory upwards
            var starts = new[] { AppContext.BaseDirectory, Directory.GetCurrentDirectory() };
            //iterate through starting points
            foreach (var start in starts)
            {
                // attempt to traverse upwards
                try
                {
                    //start from the given directory
                    var dir = new DirectoryInfo(start);
                    //traverse upwards
                    while (dir != null)
                    {
                        //check each known folder name
                        foreach (var folderName in KnownTextFolders)
                        {
                            //construct candidate path
                            var candidateFolder = Path.Combine(dir.FullName, folderName);
                            //check if it exists
                            if (Directory.Exists(candidateFolder))
                            {
                                //return the full path to Tasks.txt
                                return Path.GetFullPath(Path.Combine(candidateFolder, "Tasks.txt"));
                            }
                        }
                        //move up one directory
                        dir = dir.Parent;
                    }
                }
                catch {  }
            }

            //default to TextFiles in base directory if not found
            return Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, KnownTextFolders[0], "Tasks.txt"));
        }
        //helper to check if the path contains known text folder names another way to locate the Tasks.txt file
        private static bool PathContainsKnownTextFolder(string fullPath)
        {
            //check for null or empty
            if (string.IsNullOrEmpty(fullPath)) return false;
            //check each known folder name
            foreach (var name in KnownTextFolders)
            {
                //check for directory separators around the folder name
                if (fullPath.IndexOf(Path.DirectorySeparatorChar + name + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase) >= 0
                    || fullPath.IndexOf(Path.AltDirectorySeparatorChar + name + Path.AltDirectorySeparatorChar, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return true;
                }
            }

            return false;
        }

        public IEnumerable<TaskItem> GetAll() => _store;
        //method to add a task to the repository
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
        //method to update a task in the repository
        public void Update(TaskItem task)
        {
            if (task == null) return;

            if (_store.Contains(task))
                UpdateTaskFile();
        }
        //method to delete a task from the repository
        public void Delete(TaskItem task)
        {
            if (_store.Contains(task))
            {
                _store.Remove(task);
                UpdateTaskFile();
            }
        }
        //helper to ensure the tasks file exists before loading or saving
        private void EnsureFileExists()
        {
            var dir = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            if (!File.Exists(path))
                File.Create(path).Dispose();
        }

        //method to load tasks from the file into the in-memory store
        public void LoadTaskFile()
        {
            // Clear existing store
            _store.Clear();
            // Read file line by line
            if (!File.Exists(path))
                return;

            using StreamReader streamReader = new StreamReader(path);
            int lineNo = 0;
            while (!streamReader.EndOfStream)
            {
                // Read a line
                string? line = streamReader.ReadLine();
                lineNo++;
                // Skip empty lines
                if (string.IsNullOrWhiteSpace(line))
                    continue;
                // Trim BOM if present
                line = line.Trim();
                if (line.Length > 0 && line[0] == '\uFEFF')
                    line = line.Substring(1);
                // Try to parse the line into a TaskItem
                try
                {
                    TaskItem task = ParseLine(line);
                    _store.Add(task);
                }
                // If parsing fails, attempt fallback parsing
                catch (Exception ex)
                {
                    Debug.WriteLine($"Failed to parse task file line {lineNo}: {ex.Message}. Attempting fallback parse.");
                    // Fallback parsing
                    try
                    {
                        var fallback = FallbackParseLine(line);
                        _store.Add(fallback);
                    }
                    // If fallback also fails, log the error
                    catch (Exception inner)
                    {
                        Debug.WriteLine($"Fallback parse failed for line {lineNo}: {inner.Message}");
                    }
                }
            }
        }
        //fallback parsing method to handle malformed lines
        private TaskItem FallbackParseLine(string line)
        {
            var task = new TaskItem();
            // Extract all bracketed items first
            var itemPattern = new Regex(@"\[[^\]]*\]");
            // Find all bracketed items
            var brackets = itemPattern.Matches(line).Cast<Match>().Select(m => m.Value).ToList();
            // Replace bracketed items with a placeholder
            string lineWithoutBrackets = itemPattern.Replace(line, "__BRACKETS__");
            // Split by comma
            var parts = lineWithoutBrackets.Split(new[] { ", " }, StringSplitOptions.None);
            // Reconstruct SubTasks from placeholders
            int bracketIndex = 0;
            // Process each part
            foreach (var part in parts)
            {
                // Skip empty parts
                if (string.IsNullOrWhiteSpace(part))
                    continue;
                // Handle SubTasks specially
                if (part.StartsWith("SubTasks:"))
                {
                    // Reconstruct the full SubTasks value
                    string suffix = part.Substring("SubTasks:".Length).Trim();
                    // Rebuild from brackets
                    string reconstructed = "";
                    // Append all remaining brackets
                    for (int i = bracketIndex; i < brackets.Count; i++)
                    {
                        // Add space if needed
                        if (reconstructed.Length > 0) reconstructed += " ";
                        reconstructed += brackets[i];
                    }
                    // Use reconstructed or suffix if empty
                    var value = reconstructed == "" ? suffix : reconstructed;
                    // Parse individual SubTask items
                    var itemMatches = new Regex(@"\[(?<content>[^\]]+)\]").Matches(value);
                    // Process each SubTask item
                    foreach (Match im in itemMatches)
                    {
                        // Extract content
                        string content = im.Groups["content"].Value.Trim();
                        // Remove leading index if present
                        content = Regex.Replace(content, @"^\d+\s+", "");
                        // Match Title and Text
                        var subMatch = Regex.Match(content, @"Title: (?<title>""(?:\\.|[^""])*""|[^,]+),\s*Text: (?<text>""(?:\\.|[^""])*""|.+)$", RegexOptions.Singleline);
                        // If match found, extract Title and Text
                        if (subMatch.Success)
                        {
                            // Unquote values
                            var title = UnquoteIfQuoted(subMatch.Groups["title"].Value.Trim());
                            // Unquote values
                            var text = UnquoteIfQuoted(subMatch.Groups["text"].Value.Trim());
                            // Add to SubTasks
                            task.SubTasks.Add(new SubTask { Title = title, Text = text });
                        }
                        // If no match, add entire content as Text
                        else
                        {
                            task.SubTasks.Add(new SubTask { Title = string.Empty, Text = UnquoteIfQuoted(content) });
                        }
                    }
                    break;
                }
                // Handle other key-value pairs
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
                // Increment bracket index if placeholder was used
                if (part.Contains("__BRACKETS__") && bracketIndex < brackets.Count)
                    bracketIndex++;
            }
            
            return task;
        }
        //main parsing method to convert a line of text into a TaskItem
        private TaskItem ParseLine(string line)
        {
            // Create a new TaskItem
            var task = new TaskItem();

            // Regex pattern to match key-value pairs
            var pairPattern = new Regex(@"(?<key>\w+): (?<value>(?:\[[^\]]*\](?:\s*\[[^\]]*\])*)|[^,]*)(?:, |$)", RegexOptions.Singleline);
            // Find all matches in the line
            var matches = pairPattern.Matches(line);
            // Process each match
            foreach (Match m in matches)
            {
                // Extract key and value
                string key = m.Groups["key"].Value;
                // Trim value
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
                        // Regex pattern to match individual SubTask items
                        var itemPattern = new Regex(@"\[(?<content>[^\]]+)\]");
                        // Find all SubTask items
                        var items = itemPattern.Matches(value);
                        // Process each SubTask item
                        foreach (Match im in items)
                        {
                            // Extract content
                            string content = im.Groups["content"].Value.Trim();
                            // Remove leading index if present
                            content = Regex.Replace(content, @"^\d+\s+", "");
                            // Match Title and Text
                            var subMatch = Regex.Match(content,
                                @"Title: (?<title>""(?:\\.|[^""])*""|[^,]+),\s*Text: (?<text>""(?:\\.|[^""])*""|.+)$",
                                RegexOptions.Singleline);
                            // If match found, extract Title and Text
                            if (subMatch.Success)
                            {
                                var titleRaw = subMatch.Groups["title"].Value.Trim();
                                var textRaw = subMatch.Groups["text"].Value.Trim();
                                // Unquote values
                                var title = UnquoteIfQuoted(titleRaw);
                                var text = UnquoteIfQuoted(textRaw);
                                task.SubTasks.Add(new SubTask { Title = title, Text = text });
                            }
                            // If no match, add entire content as Text
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
        //method to update the tasks file with the current in-memory store
        private void UpdateTaskFile()
        {
            using StreamWriter streamWriter = new StreamWriter(path, false);
            // Write each task to the file
            foreach (TaskItem item in _store)
            {
                streamWriter.WriteLine(SerializeTask(item));
            }
        }
        //helper to serialize a TaskItem into a line of text
        private static string SerializeTask(TaskItem item)
        {
            // Build the string representation
            string header = $"Title: {QuoteIfNeeded(item.Title)}, Description: {QuoteIfNeeded(item.Description)}, Mail: {QuoteIfNeeded(item.Mail)}, Status: {QuoteIfNeeded(item.Status)}, Priority: {QuoteIfNeeded(item.Priority)}, Complexity: {QuoteIfNeeded(item.Complexity)}, Note: {QuoteIfNeeded(item.Note)}, SubTasks: ";
            // Handle SubTasks
            if (item.SubTasks == null || item.SubTasks.Count == 0)
                return header + "[]";
            // Build SubTasks string
            var parts = new List<string>();
            // Serialize each SubTask
            for (int i = 0; i < item.SubTasks.Count; i++)
            {
                var s = item.SubTasks[i];
                var titleQuoted = QuoteIfNeeded(s.Title);
                var textQuoted = QuoteIfNeeded(s.Text);
                parts.Add($"[{i} Title: {titleQuoted}, Text: {textQuoted}]");
            }
            // Combine and return
            return header + string.Join(" ", parts);
        }
        //helper to quote a string if it contains special characters
        private static string QuoteIfNeeded(string? input)
        {
            // Check for null or special characters
            input ??= string.Empty;
            // Determine if quoting is needed
            bool needsQuotes = input.Contains(',') || input.Contains(':') || input.Contains(']') || input.Contains('[') || input.Contains('"') || input.Contains('\\') || input.Contains('\n') || input.Contains('\r') || string.IsNullOrWhiteSpace(input);
            // If not needed, return as is
            if (!needsQuotes) return input;
            // Escape backslashes and quotes
            var escaped = input.Replace("\\", "\\\\").Replace("\"", "\\\"");
            // Return quoted string
            return $"\"{escaped}\"";
        }
        //helper to unquote a string if it is quoted
        private static string UnquoteIfQuoted(string input)
        {
            input = input?.Trim() ?? string.Empty;
            // Check if quoted
            if (input.Length >= 2 && input[0] == '"' && input[^1] == '"')
            {
                // Extract inner content
                var inner = input.Substring(1, input.Length - 2);
                // Unescape backslashes and quotes
                return inner.Replace("\\\"", "\"").Replace("\\\\", "\\");
            }
            return input;
        }
    }
}