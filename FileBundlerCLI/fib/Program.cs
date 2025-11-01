using System.CommandLine;
using System.CommandLine.Invocation;

namespace fib
{
    class Program
    {
        static async Task<int> Main(string[] args)
        {
            // create Root Command
            var rootCommand = new RootCommand("File Bundler CLI - keli le'arizat kfaze kod (tool for bundling code files)");

            // add commands
            rootCommand.AddCommand(CreateBundleCommand());
            rootCommand.AddCommand(CreateRspCommand());

            return await rootCommand.InvokeAsync(args);
        }

        // bundle command
        static Command CreateBundleCommand()
        {
            var bundleCommand = new Command("bundle", "Aroz kfaze kod le-kavetz eḥad (bundle code files into one)");

            // create options
            var languageOption = new Option<string[]>(
                aliases: new[] { "--language", "-l" },
                description: "Reshima shel sfaot tichnut (list of programming languages) or 'all' for all languages")
            {
                IsRequired = true,
                AllowMultipleArgumentsPerToken = true
            };

            var outputOption = new Option<FileInfo>(
                aliases: new[] { "--output", "-o" },
                description: "Shem shel kavetz ha-output (name of output file)")
            {
                IsRequired = true
            };

            var noteOption = new Option<bool>(
                aliases: new[] { "--note", "-n" },
                description: "include comments?)",
                getDefaultValue: () => false);

            var sortOption = new Option<string>(
                aliases: new[] { "--sort", "-s" },
                description: "Seder miun: 'name' (by name) or 'type' (by type)",
                getDefaultValue: () => "name");

            var removeEmptyLinesOption = new Option<bool>(
                aliases: new[] { "--remove-empty-lines", "-r" },
                description: "Remove shurot rekot? (empty lines)",
                getDefaultValue: () => false);

            var authorOption = new Option<string>(
                aliases: new[] { "--author", "-a" },
                description: "Shem yotzer ha-kavetz (author name)",
                getDefaultValue: () => string.Empty);

            // add options to command
            bundleCommand.AddOption(languageOption);
            bundleCommand.AddOption(outputOption);
            bundleCommand.AddOption(noteOption);
            bundleCommand.AddOption(sortOption);
            bundleCommand.AddOption(removeEmptyLinesOption);
            bundleCommand.AddOption(authorOption);

            // set handler
            bundleCommand.SetHandler(
                (languages, output, note, sort, removeEmpty, author) =>
                {
                    BundleHandler.Execute(languages, output, note, sort, removeEmpty, author);
                },
                languageOption, outputOption, noteOption, sortOption, removeEmptyLinesOption, authorOption);

            return bundleCommand;
        }

        // create-rsp command
        static Command CreateRspCommand()
        {
            var createRspCommand = new Command("create-rsp", "Tzor kavetz tshuva (response file) im pkuda muchana");

            createRspCommand.SetHandler(() =>
            {
                RspHandler.Execute();
            });

            return createRspCommand;
        }
    }
}
