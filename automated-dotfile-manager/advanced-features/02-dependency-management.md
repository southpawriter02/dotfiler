# Advanced Feature: Dependency Management

## 1. Summary

This document describes the dependency management feature for the Automated Dotfile Manager. Dotfiles are often configurations for specific command-line tools, applications, or development environments (e.g., `.vimrc` for Vim, `.zshrc` for Zsh). This feature allows the dotfile manager to be aware of these dependencies and help the user install them.

## 2. Intended Functionality

The dependency management system will:
- **Declare dependencies:** Allow users to specify the software packages that their dotfiles depend on.
- **Check for dependencies:** Provide a mechanism to check if the declared dependencies are installed on the current system.
- **Install dependencies:** Offer a way to automatically install any missing dependencies using the system's native package manager.
- **Cross-platform support:** Handle dependency definitions for multiple operating systems (e.g., specify `brew install` on macOS and `apt-get install` on Debian-based Linux).

## 3. User Stories

- *As a user,* when I set up my dotfiles on a new machine, I want the dotfile manager to tell me that I need to install `zsh`, `neovim`, and `tmux` for my configurations to work.
- *As a developer,* I want to run a command like `dotman dependencies install` and have the tool automatically install all the required packages for my dotfiles on the current OS.
- *As a user,* I want to define my dependencies in a simple file, specifying the package name for different platforms (e.g., `ripgrep` on `apt` and `brew`), so that my setup is portable.
- *As a sysadmin,* I want to get a clear report of which dependencies are met and which are missing when I run a `dotman status` or similar command.

## 4. Requirements

### Functional Requirements

- Users must be able to declare dependencies in a configuration file.
- The declaration should support mapping a conceptual dependency (e.g., "ripgrep") to its package name on different package managers (e.g., `ripgrep` on `apt` and `brew`, but `rg` might be the command).
- The tool must provide a command to check the status of all dependencies (`check`).
- The tool must provide a command to attempt to install all missing dependencies (`install`).
- The system must be able to detect the user's operating system and default package manager.
- The installation process should be interactive, asking the user for confirmation before installing packages. A `--force` or `--yes` flag could allow for non-interactive installation.

### Non-Functional Requirements

- **Reliability:** The dependency checker should be accurate. It should correctly identify whether a command or package is installed.
- **Security:** The tool must not run installation commands without user consent. It should be transparent about what commands it will execute.
- **Extensibility:** The system should be designed in a way that allows adding support for new package managers in the future.

## 5. Dependencies

### Internal Dependencies

- **CLI:** The functionality will be exposed through subcommands like `dotman dependencies check` and `dotman dependencies install`.
- **Configuration Management:** The list of dependencies will be stored in a configuration file within the dotfiles repository.

### External Dependencies

- The tool will depend on the package managers of the host system (e.g., `apt`, `yum`, `brew`, `pacman`).
- It will need a way to identify the host operating system and distribution.

## 6. Limitations & Assumptions

- This feature is not a full-blown package manager. It is a convenience layer on top of existing package managers.
- The initial implementation might only support a limited set of popular package managers (e.g., `apt`, `brew`).
- The tool will rely on the user to provide the correct package names for each platform. It will not try to guess them.
- It is assumed that the user has `sudo` or equivalent permissions to install packages.
- The dependency check might be based on checking if a binary is in the `PATH`. This may not be sufficient for all types of dependencies (e.g., libraries).

## 7. Implementation Ideas

- A new file, `dependencies.toml`, could be introduced in the dotfiles repository. This keeps the dependency information separate from the main configuration.
- The `dependencies.toml` file could have a structure like this:
  ```toml
  [[packages]]
  name = "Ripgrep"
  command = "rg" # The command to check for in the PATH

  [packages.install_with]
  apt = "ripgrep"
  brew = "ripgrep"
  pacman = "ripgrep"

  [[packages]]
  name = "Zsh"
  command = "zsh"

  [packages.install_with]
  apt = "zsh"
  yum = "zsh"
  # etc.
  ```
- The `dotman dependencies check` command would iterate through this file. For each package, it would check if `command` exists in the user's `PATH`.
- The `dotman dependencies install` command would first identify the OS and package manager. Then, for each missing dependency, it would construct and run the appropriate installation command (e.g., `sudo apt-get install -y zsh`).
- The tool could ask for `sudo` password interactively if required by the package manager.
- The logic for detecting the OS and package manager could be implemented as a helper module. It could check for the existence of `/etc/os-release` on Linux, `sw_vers` on macOS, etc.
- To avoid running package manager commands repeatedly, the tool could cache the list of installed packages for a short period.
