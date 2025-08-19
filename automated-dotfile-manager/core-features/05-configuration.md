# Core Feature: Configuration Management

## 1. Summary

This document specifies the configuration management system for the Automated Dotfile Manager. This system allows users to customize the behavior of the tool. A clear and flexible configuration system is essential for a tool that needs to adapt to different user workflows and environments.

## 2. Intended Functionality

The configuration system will manage all user-configurable settings for the dotfile manager. This includes:
- **Configuration file:** The tool will use a configuration file (e.g., in TOML or YAML format) to store its settings.
- **Settings management:** The tool will provide a way for users to view and edit these settings via the CLI.
- **Scope of configuration:** The system will support both global (user-level) and local (repository-level) configuration.
- **Key settings:** Initial settings to be managed will include:
    - The path to the managed dotfiles repository.
    - The remote URL for synchronization.
    - The default editor for opening configuration files.
    - Excluded files or directories for discovery.

## 3. User Stories

- *As a new user,* when I run `dotman init`, I want the tool to create a default configuration file for me with sensible defaults, so I can get started without much setup.
- *As a user,* I want to be able to change the location where my dotfiles are stored by editing a configuration file, for example, if I want to store them in a custom directory.
- *As a power user,* I want to run a command like `dotman config remote.url https://github.com/me/dotfiles.git` to set the remote repository for my dotfiles.
- *As a user,* I want to be able to tell the dotfile manager to always ignore certain files or directories, like `.cache`, so they are not suggested during discovery.

## 4. Requirements

### Functional Requirements

- The tool must read its configuration from a well-defined location (e.g., `~/.config/dotman/config.toml`).
- It must provide a CLI command, `dotman config`, for managing settings. This command should support:
  - `dotman config get <key>`: To view a setting.
  - `dotman config set <key> <value>`: To change a setting.
  - `dotman config list`: To view all current settings.
  - `dotman config edit`: To open the configuration file in the user's default editor.
- The configuration system must support different data types, such as strings, booleans, and arrays (e.g., for exclusion lists).
- The system should support a hierarchy of configuration files. For example, a repository-specific configuration (`.dotman/config`) could override the global user configuration (`~/.config/dotman/config.toml`).

### Non-Functional Requirements

- **Robustness:** The tool must handle missing or malformed configuration files gracefully, by falling back to default values and providing clear error messages.
- **Usability:** The configuration keys should be named logically and be well-documented.
- **Security:** The configuration file should be stored with appropriate file permissions to prevent unauthorized modification.

## 5. Dependencies

### Internal Dependencies

- **CLI:** The `config` command is part of the CLI.
- Nearly all other features will depend on the configuration system to retrieve their settings (e.g., `sync` needs the remote URL, `discovery` needs the repository path).

### External Dependencies

- A library for parsing the chosen configuration file format (e.g., a TOML or YAML parser).

## 6. Limitations & Assumptions

- The initial implementation may only support a single global configuration file. Repository-level configuration can be added later.
- The tool will not encrypt any part of the configuration file. Sensitive data should not be stored in it (see the `Secret Management` advanced feature).
- It is assumed that the user has the necessary permissions to create and write to the configuration file in their home directory.

## 7. Implementation Ideas

- TOML is a good choice for the configuration file format as it is designed for human readability.
- The configuration loader should be implemented as a module that is initialized at startup. It should load the configuration from the file, apply default values for any missing keys, and provide a simple API for other modules to access the settings.
- The `dotman config` command can be implemented as a simple key-value store editor for the configuration file.
- When `dotman init` is run, a default configuration file should be created with comments explaining each setting.
- For repository-level configuration, the tool would look for a `.dotman/config` file within the managed repository. If it exists, its settings would be merged with (and override) the global settings. This is similar to how Git handles `.git/config`.
- An example of a configuration file (`config.toml`):
  ```toml
  # The main directory where your dotfiles are stored.
  repository_path = "~/dotfiles"

  # The remote Git repository for synchronization.
  [remote]
  url = "git@github.com:user/dotfiles.git"
  branch = "main"

  # A list of patterns to ignore during discovery.
  # Uses glob patterns.
  ignore_patterns = [
      ".cache",
      ".local/share",
      "*.swp"
  ]
  ```
