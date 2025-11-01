using System;
using System.IO;
using System.Text;

namespace fib
{
    public static class RspHandler
    {
        private static readonly char[] Separators = { ',', ' ', ';' };

        public static void Execute()
        {
            Console.WriteLine("=== Creating a file of answer (Response File) ===\n");
            Console.WriteLine("Answer the following questions to create a ready command:\n");

            try
            {
                // Question 1: programming languages
                Console.Write("🔹 For example which languages in tichnut you want (csharp, java, python or 'all'):\n   ");
                string languages = Console.ReadLine()?.Trim() ?? "all";
                if (string.IsNullOrWhiteSpace(languages))
                {
                    languages = "all";
                }

                // Question 2: output file name
                Console.Write("\n🔹 What will be the output file name? (for example: bundle.txt)\n   ");
                string outputFile = Console.ReadLine()?.Trim() ?? "bundle.txt";
                if (string.IsNullOrWhiteSpace(outputFile))
                {
                    outputFile = "bundle.txt";
                }

                // Question 3: add comments
                Console.Write("\n🔹 Add notes with code source? (y/n) [default: n]\n   ");
                string noteInput = Console.ReadLine()?.Trim().ToLower() ?? "n";
                bool includeNote = noteInput == "y" || noteInput == "yes" || noteInput == "כן";

                // Question 4: sort order
                // Question 4: sort order
                Console.Write("\n🔹 How to sort the files? (name/type) [default: name]\n   ");
                string sortInput = Console.ReadLine()?.Trim().ToLower();

                // If user presses Enter, use default "name"
                string sort = string.IsNullOrWhiteSpace(sortInput) ? "name" : sortInput;

                // Validate only if user entered something
                while (!string.IsNullOrWhiteSpace(sortInput) && sort != "name" && sort != "type")
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.Write("❌ Wrong value. Please write 'name' or 'type' (or press Enter for default): ");
                    Console.ResetColor();
                    sortInput = Console.ReadLine()?.Trim().ToLower();
                    sort = string.IsNullOrWhiteSpace(sortInput) ? "name" : sortInput;
                }

                // validation
                while (sort != "name" && sort != "type")
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.Write("❌ Wrong value. Please write 'name' or 'type': ");
                    Console.ResetColor();
                    sort = Console.ReadLine()?.Trim().ToLower() ?? "name";
                }

                // Question 5: remove empty lines
                Console.Write("\n🔹 Remove empty lines? (y/n) [default: n]\n   ");
                string removeEmptyInput = Console.ReadLine()?.Trim().ToLower() ?? "n";
                bool removeEmpty = removeEmptyInput == "y" || removeEmptyInput == "yes" || removeEmptyInput == "כן";

                // Question 6: author name
                Console.Write("\n🔹 Author name (optional, press Enter to skip):\n   ");
                string author = Console.ReadLine()?.Trim() ?? "";

                // Question 7: response file name
                Console.Write("\n🔹 What will be the name of the Response file (for example: commands.rsp)\n   ");
                string rspFileName = Console.ReadLine()?.Trim() ?? "commands.rsp";
                if (string.IsNullOrWhiteSpace(rspFileName))
                {
                    rspFileName = "commands.rsp";
                }
                if (!rspFileName.EndsWith(".rsp", StringComparison.OrdinalIgnoreCase))
                {
                    rspFileName += ".rsp";
                }

                // build the command
                var commandBuilder = new StringBuilder();
                commandBuilder.AppendLine("bundle");

                // add languages
                string[] langs = languages.Split(Separators, StringSplitOptions.RemoveEmptyEntries);
                commandBuilder.Append("--language");
                foreach (var lang in langs)
                {
                    commandBuilder.Append($" {lang.Trim()}");
                }
                commandBuilder.AppendLine();

                // add output file
                if (outputFile.Contains(' '))
                {
                    commandBuilder.AppendLine($"--output \"{outputFile}\"");
                }
                else
                {
                    commandBuilder.AppendLine($"--output {outputFile}");
                }

                // boolean options
                if (includeNote)
                {
                    commandBuilder.AppendLine("--note");
                }

                if (removeEmpty)
                {
                    commandBuilder.AppendLine("--remove-empty-lines");
                }

                // add sort
                commandBuilder.AppendLine($"--sort {sort}");

                // add author
                if (!string.IsNullOrWhiteSpace(author))
                {
                    if (author.Contains(' '))
                    {
                        commandBuilder.AppendLine($"--author \"{author}\"");
                    }
                    else
                    {
                        commandBuilder.AppendLine($"--author {author}");
                    }
                }

                // save file
                string rspFilePath = Path.Combine(Directory.GetCurrentDirectory(), rspFileName);

                try
                {
                    File.WriteAllText(rspFilePath, commandBuilder.ToString(), Encoding.UTF8);
                }
                catch (UnauthorizedAccessException)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"\n❌ Error: no permission to write in location: {Directory.GetCurrentDirectory()}");
                    Console.ResetColor();
                    return;
                }
                catch (IOException ex)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"\n❌ Error saving the file: {ex.Message}");
                    Console.ResetColor();
                    return;
                }

                // summary
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("\n✅ Response file created successfully!");
                Console.ResetColor();

                Console.WriteLine($"\n📄 File location: {rspFilePath}");
                Console.WriteLine("\n📋 Command content:");
                Console.WriteLine("─────────────────────────────────────");
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine(commandBuilder.ToString());
                Console.ResetColor();
                Console.WriteLine("─────────────────────────────────────");

                Console.WriteLine("\n💡 To run the command, type:");
                Console.ForegroundColor = ConsoleColor.Yellow;

                // get exe name
                string exeName = System.Diagnostics.Process.GetCurrentProcess().ProcessName;
                Console.WriteLine($"   {exeName} @{rspFileName}");
                Console.ResetColor();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"\n❌ Error creating the file: {ex.Message}");
                Console.ResetColor();
            }
        }
    }
}
