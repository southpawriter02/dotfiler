# Technical Architecture: Data Model

## 1. Summary

This document describes the data model for the Automated Dotfile Manager. The data model defines the structure and relationships of the data that the application will manage. A well-defined data model is crucial for ensuring data integrity, consistency, and for providing a solid foundation for the application's logic.

## 2. Core Data Structures

The application's data can be divided into two main categories: data stored within the dotfiles repository itself, and data stored locally on the machine (outside the repository).

### 2.1. Repository Data Model (Version Controlled)

This data is stored within the managed dotfiles repository and is synced across machines.

**`manifest.json` (or `manifest.toml`)**
This is the central file that defines which dotfiles are being tracked. It is the "source of truth" for the repository's contents.

- **Type:** JSON or TOML file.
- **Location:** Root of the dotfiles repository.
- **Content:** An object or map where keys are the tracked files' original paths (relative to `~`) and values are objects containing metadata.

*Example `manifest.json`:*
```json
{
  ".zshrc": {
    "source": "dotfiles/.zshrc",
    "profile": "common",
    "is_template": true
  },
  ".config/nvim/init.vim": {
    "source": "dotfiles/.config/nvim/init.vim",
    "profile": "dev",
    "is_template": false
  },
  ".secret_api_key": {
    "source": "secrets/.secret_api_key.gpg",
    "profile": "common",
    "is_secret": true
  }
}
```

**`config.toml` (Repository Level)**
This file stores configuration settings that are specific to the repository and should be consistent across all machines using it.

- **Type:** TOML file.
- **Location:** `.dotman/config.toml` within the repository.
- **Content:** Overrides for global settings, profile definitions, template variables.

*Example `.dotman/config.toml`:*
```toml
# Variables for the templating engine
[variables]
  name = "Jules"

# Profile-specific variables
[profiles.work.variables]
  email = "jules@work.com"
```

**`dependencies.toml`**
Defines the software dependencies for the dotfiles in the repository.

- **Type:** TOML file.
- **Location:** Root of the repository, or per-profile.
- **Content:** A list of packages with their names and installation commands for different platforms.

*Example `dependencies.toml`:*
```toml
[[packages]]
  name = "Zsh"
  command = "zsh"
  [packages.install_with]
    apt = "zsh"
    brew = "zsh"
```

### 2.2. Local State Data Model (Machine Specific)

This data is stored on the local machine and is *not* version controlled, as it pertains to the state of a single machine.

**`~/.config/dotman/config.toml` (Global Config)**
The main configuration file for the tool on a given machine.

- **Type:** TOML file.
- **Location:** A standard user config directory.
- **Content:** Global settings like the path to the dotfiles repository, user's GPG key ID, etc.

*Example global `config.toml`:*
```toml
# Path to the local clone of your dotfiles repository
repository_path = "/Users/jules/dotfiles"

[secrets]
  gpg_key_id = "your_gpg_key_id"
```

**`~/.config/dotman/state.json` (Local State)**
Stores the state of the dotfile manager on the local machine.

- **Type:** JSON file.
- **Location:** A standard user config directory.
- **Content:** Information about the current machine's state, such as the active profile.

*Example `state.json`:*
```json
{
  "machine_id": "a-unique-machine-id",
  "last_sync": "2023-10-27T10:00:00Z",
  "active_profile": "work"
}
```

## 3. Data Flow and Relationships

1.  **Initialization (`init`):**
    - The global `config.toml` is created, and the `repository_path` is set.
    - Inside the repository, the `manifest.json` is created (initially empty).
    - A Git repository is initialized.

2.  **Adding a file (`add`):**
    - A new entry is added to `manifest.json`.
    - The file is moved into the repository.
    - A symlink is created at the original location.
    - The changes to `manifest.json` and the new file are committed to Git.

3.  **Switching a profile (`profile activate`):**
    - The `active_profile` value in the local `state.json` is updated.
    - The tool reads the `manifest.json` and symlinks all files belonging to the new active profile (and the "common" profile).

4.  **Synchronization (`sync`):**
    - The tool pulls changes from the remote Git repository.
    - It reads the updated `manifest.json` and applies changes (new symlinks, removing old ones).
    - It may trigger the templating engine if template files or variables have changed.
    - It may trigger the secret manager if secret files have changed.
    - It updates the `last_sync` timestamp in the local `state.json`.

## 4. Rationale

- **Separation of Concerns:** The model clearly separates version-controlled configuration (which should be the same everywhere) from local state (which is unique to each machine). This is a critical design principle for dotfile management.
- **Source of Truth:** The `manifest.json` acts as a single, declarative source of truth for what is being managed. This makes the system's behavior predictable and auditable.
- **Human-Readable Formats:** Using TOML and JSON makes the configuration and manifest files easy for users to inspect and edit manually if needed.
- **Extensibility:** The data model is designed to be extensible. Adding new features like profiles or dependencies involves adding new, structured data to these files rather than requiring complex changes to the core logic. For example, adding a new backend would mean the global `config.toml` would specify the backend type and its specific configuration, but the `manifest.json` would remain largely the same.
