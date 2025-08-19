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

        var repoUrlArgument = new Argument<string>("repo-url", "The URL of the remote Git repository to install from.");
        var installCommand = new Command("install", "Install dotfiles from a remote repository.")
        {
            repoUrlArgument
        };

        rootCommand.Subcommands.Add(initCommand);
        rootCommand.Subcommands.Add(addCommand);
        rootCommand.Subcommands.Add(removeCommand);
        rootCommand.Subcommands.Add(listCommand);
        rootCommand.Subcommands.Add(syncCommand);
        rootCommand.Subcommands.Add(statusCommand);
        rootCommand.Subcommands.Add(configCommand);
        rootCommand.Subcommands.Add(installCommand);

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

        installCommand.SetAction(async (parseResult) =>
        {
            var repoUrl = parseResult.GetValue(repoUrlArgument);
            if (string.IsNullOrEmpty(repoUrl))
            {
                Console.Error.WriteLine("Error: Repository URL not specified.");
                return;
            }

            if (Directory.Exists(repoPath))
            {
                Console.Error.WriteLine($"Error: Directory '{repoPath}' already exists. The install command is for setting up a new machine.");
                return;
            }

            Console.WriteLine($"Cloning repository from {repoUrl} into {repoPath}...");
            await gitBackend.CloneAsync(repoUrl, repoPath);
            Console.WriteLine("Repository cloned successfully.");

            var newManifestService = new ManifestService(repoPath);
            await newManifestService.LoadOrCreateAsync();
            Console.WriteLine("Manifest loaded.");

            Console.WriteLine("Creating symbolic links...");
            var homePath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

            foreach (var entry in newManifestService.Manifest.Entries)
            {
                var repoFilePath = Path.Combine(repoPath, entry.Value.Source);
                var targetPath = Path.Combine(homePath, entry.Key);

                // Ensure the target directory exists
                var targetDir = Path.GetDirectoryName(targetPath);
                if (!string.IsNullOrEmpty(targetDir))
                {
                    Directory.CreateDirectory(targetDir);
                }

                // Backup existing file if it's not already a symlink to the right place
                if (File.Exists(targetPath) || Directory.Exists(targetPath))
                {
                    if (File.ResolveLinkTarget(targetPath, returnFinalTarget: true)?.FullName.Equals(repoFilePath, StringComparison.OrdinalIgnoreCase) ?? false)
                    {
                        Console.WriteLine($"Link already exists for {entry.Key}. Skipping.");
                        continue;
                    }

                    var backupPath = targetPath + ".dotman-backup";
                    Console.WriteLine($"Backing up existing file at {targetPath} to {backupPath}");
                    File.Move(targetPath, backupPath, overwrite: true);
                }

                try
                {
                    Console.WriteLine($"Creating link: {targetPath} -> {repoFilePath}");
                    File.CreateSymbolicLink(targetPath, repoFilePath);
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine($"Failed to create symlink for {entry.Key}: {ex.Message}");
                    // Attempt to restore backup
                    var backupPath = targetPath + ".dotman-backup";
                    if (File.Exists(backupPath))
                    {
                        File.Move(backupPath, targetPath, overwrite: true);
                        Console.WriteLine($"Restored backup for {targetPath}.");
                    }
                }
            }
            Console.WriteLine("Installation complete.");
        });

        return await rootCommand.Parse(args).InvokeAsync();
    }
}
