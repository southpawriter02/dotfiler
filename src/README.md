# Dotman - A Dotfile Manager

This is a command-line tool for managing dotfiles, built in C# with .NET. It is based on the detailed project roadmap provided.

## Current Status

The core framework of the application is complete. The following features have been implemented:

- **Core Logic Library (`Dotman.Core`)**: A library containing all the business logic for managing dotfiles.
- **CLI (`Dotman.Console`)**: A command-line interface for interacting with the core logic.
- **Configuration**: The tool can be configured via `~/.config/dotman/config.toml`.
- **Git Backend**: All file operations are tracked in a Git repository.
- **Testing**: A testing project is set up with an initial test for the configuration manager.

## How to Build and Run

1.  **Prerequisites**:
    - .NET 9 SDK (or later)
    - Git

2.  **Build the solution**:
    ```bash
    dotnet build DotfileManager.sln
    ```

3.  **Run the application**:
    The executable can be found in `Dotman.Console/bin/Debug/net9.0/`.
    ```bash
    ./Dotman.Console/bin/Debug/net9.0/dotman <command>
    ```

## Available Commands

- `dotman init`: Initializes a new dotfiles repository in the location specified in the config file.
- `dotman add <file>`: Adds a file to be tracked.
- `dotman remove <file>`: Stops tracking a file.
- `dotman list`: Lists all currently tracked files.
- `dotman sync`: Synchronizes the local repository with the remote (pulls and pushes).
- `dotman status`: Shows the current status of the local repository.
- `dotman config`: (Not yet implemented) Manage configuration.
