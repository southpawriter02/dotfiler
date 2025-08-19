# Feature: Interactive Mode

## Description

An interactive mode for the command-line interface (CLI) to make the tool more user-friendly, especially for new users. This mode will guide users through common tasks with prompts and menus.

## Intended Functionality

- **Interactive `add` command**: An `add --interactive` or `add-interactive` command that:
    - Scans the user's home directory for common dotfiles.
    - Presents a checklist or a fuzzy-findable list of potential dotfiles to the user.
    - Allows the user to select multiple files to be added to tracking at once.
- **Interactive `init` command**: A guided process for the `init` command that asks the user for the repository location, remote URL, and other initial configuration settings.
- **Interactive Conflict Resolution**: When a synchronization conflict occurs, the tool will present an interactive prompt asking the user how they want to resolve it (e.g., "keep local", "keep remote", "open diff tool").
- **Helpful Prompts**: Throughout the application, helpful prompts and suggestions will be provided to guide the user.

## Requirements

- A library for creating interactive command-line interfaces (e.g., `inquirer.js` for Node.js, `rich` or `prompt_toolkit` for Python).

## Limitations

- Interactive mode is not suitable for scripting or automation. The tool must still support non-interactive commands with flags for these use cases.

## Dependencies

- **`core/dotfile-tracking.md`**: The interactive `add` command will build upon the basic tracking functionality.
- **`core/synchronization.md`**: The interactive conflict resolution will be part of the synchronization process.
