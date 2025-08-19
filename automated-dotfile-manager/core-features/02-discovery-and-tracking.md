# Core Feature: Dotfile Discovery and Tracking

## 1. Summary

This feature enables the Automated Dotfile Manager to identify, track, and manage the dotfiles in a user's home directory. It is the core mechanism by which files are brought under the control of the manager. This document specifies how the tool will discover both explicit and implicit dotfiles and how it maintains a list of tracked files.

## 2. Intended Functionality

The discovery and tracking system will be responsible for:
- **Adding files:** Users can explicitly specify which files to track. The tool will move the file to its own managed directory and create a symbolic link (symlink) in the original location.
- **Removing files:** Users can stop tracking a file. The tool will remove the file from its managed directory and, optionally, restore the original file from the symlink.
- **Listing files:** The tool will maintain a manifest of all tracked files, which can be queried by the user.
- **Automatic discovery (optional):** The tool could have a mechanism to scan the user's home directory for common dotfiles and suggest them for tracking.

## 3. User Stories

- *As a user,* I want to tell the dotfile manager to `add` my `.bashrc` file, so that it can be version-controlled and synced across my machines.
- *As a user,* I want to be able to `remove` a file from the dotfile manager's tracking if I no longer want to manage it, and I expect the original file to be put back in its place.
- *As a user,* I want to `list` all the files that are currently being managed, so I can get a quick overview of my setup.
- *As a new user,* I would appreciate a feature that scans my home directory and suggests files like `.gitconfig` or `.zshrc` to add, making the initial setup easier.

## 4. Requirements

### Functional Requirements

- The tool must be able to track any file or directory within the user's home directory.
- When a file is added:
  - It must be moved (or copied) to a dedicated, managed repository directory.
  - A symlink must be created at the original location of the file, pointing to the file in the managed repository.
  - The tool must handle cases where a file with the same name already exists in the repository.
- When a file is removed:
  - The symlink at the original location must be removed.
  - The file in the managed repository must be deleted.
  - The user should be given the option to restore the file to its original location.
- The tool must maintain a manifest file that lists all tracked files and their original locations.
- The system must be able to handle file and directory name collisions.

### Non-Functional Requirements

- **Reliability:** The process of adding and removing files must be atomic to prevent data loss. For example, if creating a symlink fails, the original file should not be left in a moved state without a link.
- **Performance:** For a typical number of dotfiles (<100), listing and searching should be instantaneous.
- **Security:** The tool must handle file permissions correctly, ensuring that moved files and new symlinks retain appropriate permissions.

## 5. Dependencies

### Internal Dependencies

- **CLI:** This feature will be exposed through CLI commands like `add`, `remove`, and `list`.
- **Configuration Management:** The location of the managed repository directory will be stored in the tool's configuration.
- **Version Control Integration:** The managed repository directory will likely be a Git repository.

### External Dependencies

- None for the core logic, but the implementation will rely on filesystem operations of the host operating system (e.g., for creating symlinks).

## 6. Limitations & Assumptions

- This feature assumes that the use of symlinks is acceptable. Some systems or filesystems may have limitations with symlinks.
- The initial implementation may not support tracking files outside the user's home directory.
- The automatic discovery feature might be limited to a predefined list of common dotfiles to avoid suggesting irrelevant files.
- It is assumed that the user has the necessary permissions to move files and create symlinks in their home directory.

## 7. Implementation Ideas

- A JSON or TOML file within the managed repository can serve as the manifest. This file would map original file paths to their paths within the repository.
- The core logic could be implemented as a library/module with a clear API (`add_file`, `remove_file`, etc.) that the CLI can call.
- For reliability, operations should be broken down into steps with clear rollback procedures. For example, when adding a file:
  1. Copy the file to the repository.
  2. Verify the copy was successful.
  3. Create the symlink.
  4. If symlink creation succeeds, delete the original file. If it fails, delete the copied file from the repository and report an error.
- The automatic discovery could work by scanning the home directory (`~`) for files and directories starting with a dot (`.`), excluding some common directories like `.cache` or `.config`. It would then compare this list against the list of already tracked files.
