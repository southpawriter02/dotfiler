# Feature: Dotfile Tracking

## Description

This feature allows the user to specify which dotfiles they want to manage. The tool will keep track of these files and their locations. This is the foundational feature upon which all other functionalities are built.

## Intended Functionality

- **Add a dotfile to be tracked**: A command to add a new dotfile to the list of managed files. The tool should handle copying the file to a central repository.
- **Remove a dotfile from tracking**: A command to stop tracking a dotfile. The user should be given the option to either delete the file from the central repository or leave it as is.
- **List tracked dotfiles**: A command to display all the dotfiles currently being managed.
- **Configuration File**: The list of tracked files will be stored in a configuration file (e.g., `dotfiles.json` or `dotfiles.toml`) within the dotfile repository. This file will map the file in the repository to its intended location on the user's system.

## Requirements

- A command-line interface (CLI) to interact with the tracking functionality.
- A mechanism to store the list of tracked files.

## Limitations

- This feature on its own does not handle synchronization or versioning. It only keeps track of which files to manage.
- It will initially only support files, not directories, but directory support should be a planned enhancement.

## Dependencies

- None. This is a core, foundational feature.
