using System.CommandLine;
using Dotman.Core;

class Program
{
    static async Task<int> Main(string[] args)
    {
        // 1. Instantiate Core Services
        var configManager = new ConfigManager();
        await configManager.LoadAsync();

        var repoPath = configManager.Get("repositorypath");
        var gitBackend = new GitBackend(repoPath);
        var manifestService = new ManifestService(repoPath);
        // Tracking and Sync modules will be needed for other commands.
        var trackingModule = new TrackingModule(repoPath, manifestService, gitBackend);
        var syncModule = new SyncModule(gitBackend);

        // 2. Define CLI Commands
        var rootCommand = new RootCommand("A simple command-line tool for managing dotfiles.");

        var initCommand = new Command("init", "Initialize a new dotfiles repository.");

        var fileArgument = new Argument<FileInfo>("file")
        {
            Description = "The file to track."
        };
        var addCommand = new Command("add", "Add a file to be tracked.")
        {
            fileArgument
        };

        var fileToRemoveArgument = new Argument<string>("file") { Description = "The file to stop tracking." };
        var removeCommand = new Command("remove", "Stop tracking a file.")
        {
            fileToRemoveArgument
        };
        var listCommand = new Command("list", "List all tracked files.");
        var syncCommand = new Command("sync", "Synchronize with the remote repository.");
        var statusCommand = new Command("status", "Show the status of tracked files.");
        var configCommand = new Command("config", "Manage configuration.");

        rootCommand.Subcommands.Add(initCommand);
        rootCommand.Subcommands.Add(addCommand);
        rootCommand.Subcommands.Add(removeCommand);
        rootCommand.Subcommands.Add(listCommand);
        rootCommand.Subcommands.Add(syncCommand);
        rootCommand.Subcommands.Add(statusCommand);
        rootCommand.Subcommands.Add(configCommand);

        // 3. Wire up Command Handlers (Actions)
        initCommand.SetAction(async (parseResult) =>
        {
            Console.WriteLine($"Initializing dotfiles repository at: {repoPath}");
            Directory.CreateDirectory(repoPath);
            await gitBackend.InitAsync();
            await manifestService.LoadOrCreateAsync();
            Console.WriteLine("Repository initialized successfully.");
        });

        addCommand.SetAction(async (parseResult) =>
        {
            var file = parseResult.GetValue(fileArgument);
            if (file == null || !file.Exists)
            {
                Console.Error.WriteLine("Error: File not found.");
                return;
            }
            await trackingModule.AddFileAsync(file.FullName);
            Console.WriteLine($"Successfully added {file.Name} to tracking.");
        });

        removeCommand.SetAction(async (parseResult) =>
        {
            var file = parseResult.GetValue(fileToRemoveArgument);
            if (string.IsNullOrEmpty(file))
            {
                Console.Error.WriteLine("Error: File not specified.");
                return;
            }
            await manifestService.LoadOrCreateAsync();
            await trackingModule.RemoveFileAsync(file);
            Console.WriteLine($"Successfully stopped tracking {file}.");
        });

        listCommand.SetAction(async (parseResult) =>
        {
            await manifestService.LoadOrCreateAsync(); // Ensure manifest is loaded
            var files = await trackingModule.ListTrackedFilesAsync();
            Console.WriteLine("Tracked files:");
            foreach (var file in files)
            {
                Console.WriteLine(file);
            }
        });

        syncCommand.SetAction(async (parseResult) =>
        {
            Console.WriteLine("Syncing with remote repository...");
            await syncModule.SyncAsync();
            Console.WriteLine("Sync complete.");
        });

        statusCommand.SetAction(async (parseResult) =>
        {
            Console.WriteLine("Checking status...");
            var status = await syncModule.GetRemoteStatusAsync();
            Console.WriteLine("Current status:");
            Console.WriteLine(status);
        });

        configCommand.SetAction(parseResult => { Console.WriteLine("Config command called."); });

        return await rootCommand.Parse(args).InvokeAsync();
    }
}
