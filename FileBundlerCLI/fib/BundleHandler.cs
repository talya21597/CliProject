using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace fib
{
    public static class BundleHandler
    {
        // mapping of extensions to programming languages - extended list
        private static readonly Dictionary<string, string[]> LanguageExtensions = new()
        {
            { "csharp", new[] { ".cs" } },
            { "java", new[] { ".java" } },
            { "python", new[] { ".py" } },
            { "javascript", new[] { ".js", ".jsx" } },
            { "typescript", new[] { ".ts", ".tsx" } },
            { "html", new[] { ".html", ".htm" } },
            { "css", new[] { ".css", ".scss", ".sass", ".less" } },
            { "cpp", new[] { ".cpp", ".cc", ".cxx", ".h", ".hpp", ".hxx" } },
            { "c", new[] { ".c", ".h" } },
            { "php", new[] { ".php" } },
            { "ruby", new[] { ".rb" } },
            { "go", new[] { ".go" } },
            { "rust", new[] { ".rs" } },
            { "sql", new[] { ".sql" } },
            { "swift", new[] { ".swift" } },
            { "kotlin", new[] { ".kt", ".kts" } },
            { "r", new[] { ".r", ".R" } },
            { "perl", new[] { ".pl", ".pm" } },
            { "shell", new[] { ".sh", ".bash" } },
            { "powershell", new[] { ".ps1", ".psm1" } },
            { "xml", new[] { ".xml", ".xaml" } },
            { "json", new[] { ".json" } },
            { "yaml", new[] { ".yaml", ".yml" } },
            { "markdown", new[] { ".md" } }
        };

        // folders to exclude
        private static readonly string[] ExcludedFolders =
        {
            "bin", "obj", "debug", "release", ".vs", ".git",
            "node_modules", "packages", ".vscode", ".idea",
            "target", "build", "dist", "out"
        };

        public static void Execute(string[] languages, FileInfo output, bool includeNote, string sortBy, bool removeEmptyLines, string author)
        {
            try
            {
                Console.WriteLine("Starting bundling process...\n");

                // validate output file
                if (string.IsNullOrWhiteSpace(output.Name))
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("❌ Error: output file name cannot be empty.");
                    Console.ResetColor();
                    return;
                }

                // current directory
                string currentDirectory = Directory.GetCurrentDirectory();
                Console.WriteLine($"Scanning directory: {currentDirectory}\n");

                // validate languages
                if (languages == null || languages.Length == 0)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("❌ Error: please specify at least one language.");
                    Console.ResetColor();
                    return;
                }

                // get requested extensions
                List<string> extensions = GetExtensions(languages);
                if (extensions.Count == 0)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("❌ Error: no valid extensions found for selected languages.");
                    Console.WriteLine("💡 Supported languages: csharp, java, python, javascript, typescript, html, css, cpp, c, php, ruby, go, rust, sql, swift, kotlin, r, perl, shell, powershell, xml, json, yaml, markdown");
                    Console.ResetColor();
                    return;
                }

                // collect files
                List<FileInfo> files = GetCodeFiles(currentDirectory, extensions);

                if (files.Count == 0)
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("⚠️ No matching code files found.");
                    Console.ResetColor();
                    return;
                }

                Console.WriteLine($"Found {files.Count} files:\n");

                // sort files
                files = SortFiles(files, sortBy);

                // create output bundle
                CreateBundleFile(files, output, includeNote, removeEmptyLines, author, currentDirectory);

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"\n✅ Bundling completed successfully!");
                Console.WriteLine($"📄 File saved at: {output.FullName}");
                Console.ResetColor();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"\n❌ Error: {ex.Message}");
                Console.ResetColor();
            }
        }

        private static List<string> GetExtensions(string[] languages)
        {
            List<string> extensions = new List<string>();

            // if "all" is selected
            if (languages.Any(l => l.Equals("all", StringComparison.OrdinalIgnoreCase)))
            {
                foreach (var exts in LanguageExtensions.Values)
                {
                    extensions.AddRange(exts);
                }
            }
            else
            {
                // add extensions for chosen languages
                foreach (var lang in languages)
                {
                    string langLower = lang.ToLower().Trim();
                    if (LanguageExtensions.TryGetValue(langLower, out string[]? value))
                    {
                        extensions.AddRange(value);
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine($"⚠️ Unknown language: {lang}");
                        Console.ResetColor();
                    }
                }
            }

            return extensions.Distinct().ToList();
        }

        private static List<FileInfo> GetCodeFiles(string directory, List<string> extensions)
        {
            List<FileInfo> files = new List<FileInfo>();
            var dirInfo = new DirectoryInfo(directory);

            try
            {
                // collect files in current folder
                foreach (var ext in extensions)
                {
                    files.AddRange(dirInfo.GetFiles($"*{ext}", SearchOption.TopDirectoryOnly));
                }

                // collect files from subdirectories (excluding excluded folders)
                foreach (var subDir in dirInfo.GetDirectories())
                {
                    if (!ExcludedFolders.Any(excluded =>
                        subDir.Name.Equals(excluded, StringComparison.OrdinalIgnoreCase)))
                    {
                        files.AddRange(GetCodeFiles(subDir.FullName, extensions));
                    }
                }
            }
            catch (UnauthorizedAccessException)
            {
                // skip folders without access
            }

            return files;
        }

        private static List<FileInfo> SortFiles(List<FileInfo> files, string sortBy)
        {
            if (sortBy.Equals("type", StringComparison.OrdinalIgnoreCase))
            {
                return files.OrderBy(f => f.Extension).ThenBy(f => f.Name).ToList();
            }
            else
            {
                return files.OrderBy(f => f.Name).ToList();
            }
        }

        private static readonly string[] LineSeparators = { "\r\n", "\r", "\n" };

        private static void CreateBundleFile(List<FileInfo> files, FileInfo output, bool includeNote,
            bool removeEmptyLines, string author, string baseDirectory)
        {
            try
            {
                // create directory if not exists
                if (output.Directory != null && !output.Directory.Exists)
                {
                    try
                    {
                        output.Directory.Create();
                    }
                    catch (Exception ex)
                    {
                        throw new Exception($"Cannot create directory: {output.Directory.FullName}. {ex.Message}");
                    }
                }

                using var writer = new StreamWriter(output.FullName, false, Encoding.UTF8);

                // write header with author
                if (!string.IsNullOrWhiteSpace(author))
                {
                    writer.WriteLine($"// Bundle created by: {author}");
                    writer.WriteLine($"// Created on: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                    writer.WriteLine();
                }

                // write file contents
                foreach (var file in files)
                {
                    Console.WriteLine($"  📄 {file.Name}");

                    // note about source
                    if (includeNote)
                    {
                        string relativePath = Path.GetRelativePath(baseDirectory, file.FullName);
                        writer.WriteLine($"// Source: {relativePath}");
                        writer.WriteLine($"// ----------------------------------------");
                    }

                    string content = File.ReadAllText(file.FullName);

                    if (removeEmptyLines)
                    {
                        var lines = content.Split(LineSeparators, StringSplitOptions.None)
                                          .Where(line => !string.IsNullOrWhiteSpace(line));
                        content = string.Join(Environment.NewLine, lines);
                    }

                    writer.WriteLine(content);
                    writer.WriteLine();
                    writer.WriteLine();
                }
            }
            catch (UnauthorizedAccessException)
            {
                throw new Exception($"No write permissions at location: {output.DirectoryName}");
            }
            catch (DirectoryNotFoundException)
            {
                throw new Exception($"Directory not found: {output.DirectoryName}");
            }
            catch (IOException ex)
            {
                throw new Exception($"Error writing file: {ex.Message}");
            }
        }
    }
}
