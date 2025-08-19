# Advanced Feature: Profiles and Contexts

## 1. Summary

This document describes the "Profiles and Contexts" feature for the Automated Dotfile Manager. This allows a user to maintain multiple, distinct sets of dotfiles within a single repository and switch between them. This is useful for separating work and personal environments, or for managing configurations for different roles (e.g., "web-dev", "data-science", "gaming").

## 2. Intended Functionality

The profiles feature will enable users to:
- **Define profiles:** A user can group a set of dotfiles and their configurations into a named profile.
- **Switch between profiles:** A user can activate a specific profile on a machine. Activating a profile would symlink the dotfiles associated with that profile into the home directory.
- **Layer profiles:** Profiles could be layered on top of each other. For example, a user could have a "base" profile with common settings, and a "work" profile that adds or overrides specific configurations.
- **Associate dependencies and variables:** Each profile could have its own set of dependencies and template variables.

## 3. User Stories

- *As a freelancer,* I want to have a "personal" profile for my own projects and a "client-A" profile for my work with a specific client. I want to be able to easily switch my shell and editor configurations when I switch contexts.
- *As a developer,* I want a "base" profile with my `.gitconfig` and `.zshrc`, and then layer a "python-dev" profile on top of it that adds a `.vimrc` optimized for Python and a `requirements.txt` for my common tools.
- *As a user,* when I set up a new work machine, I want to be able to run a single command like `dotman profile activate work` to deploy all my work-related dotfiles and install their dependencies.
- *As a user,* I want to list all available profiles in my repository so I know which ones I can switch to.

## 4. Requirements

### Functional Requirements

- The tool must allow users to create, delete, and list profiles.
- A command must be provided to switch the active profile on a machine, e.g., `dotman profile activate <profile-name>`.
- When a profile is activated:
  - Any dotfiles from the previously active profile should be unlinked.
  - The dotfiles from the new profile should be symlinked into place.
- The system must support a "base" or "common" set of dotfiles that are always active, regardless of the selected profile.
- Each profile should be able to have its own:
  - Set of tracked files.
  - Template variables.
  - Dependencies.
- The current active profile should be stored in the local (not repository) configuration, as it's specific to a machine.

### Non-Functional Requirements

- **Performance:** Switching between profiles should be a fast operation.
- **Usability:** The concept of profiles should be easy to understand and manage. The CLI commands should be intuitive.
- **Atomicity:** The process of switching profiles should be atomic. The tool should not leave the system in a broken state with a mix of dotfiles from two different profiles.

## 5. Dependencies

### Internal Dependencies

- **CLI:** Profile management will be exposed through the `dotman profile` subcommand.
- **Discovery and Tracking:** The tracking system will need to be aware of which files belong to which profile.
- **Templating:** The templating engine will need to use variables from the active profile.
- **Dependency Management:** The dependency manager will need to check and install dependencies for the active profile.

### External Dependencies

- None. This is a purely logical feature built on top of the core functionalities.

## 6. Limitations & Assumptions

- The initial implementation might not support layering or inheritance of profiles. It might start with mutually exclusive profiles.
- Managing conflicts between profiles (e.g., if two profiles want to manage the same dotfile) can be complex. The initial design might enforce that a given dotfile can only belong to one profile.
- This feature adds significant complexity to the tool. It might be better suited for power users.

## 7. Implementation Ideas

- The directory structure of the dotfiles repository could be used to define profiles. For example:
  ```
  /dotfiles-repo
  ├── common/
  │   └── .gitconfig
  ├── profiles/
  │   ├── work/
  │   │   ├── .vimrc
  │   │   └── .zshrc_work
  │   ├── personal/
  │   │   ├── .vimrc
  │   │   └── .config/htop/htoprc
  ├── dependencies.toml
  └── profiles/work/dependencies.toml
  ```
  In this structure, `common` contains base files. The `profiles` directory contains subdirectories for each profile.
- A manifest file could explicitly define which files belong to which profile. This might be more flexible than relying on directory structure.
- The `dotman profile activate work` command would:
  1. Read the local state to see which profile is currently active (if any).
  2. Unlink all symlinks associated with the old profile.
  3. Link all files from the `common` group.
  4. Link all files from the `work` profile. If a file exists in both `common` and `work` (like `.vimrc` above), the profile-specific one takes precedence.
  5. Trigger the dependency manager to check dependencies for the `work` profile.
  6. Trigger the templating engine to re-render templates using variables from the `work` profile.
- The state of the active profile would be stored in a file like `~/.config/dotman/state.json`, which is outside the version-controlled repository.
- The `config.toml` could be extended to support profile-specific variables:
  ```toml
  [variables] # Common variables
  name = "Jules"

  [profiles.work.variables]
  email = "jules@work.com"

  [profiles.personal.variables]
  email = "jules@home.com"
  ```
  The templating engine would merge these variables based on the active profile.
