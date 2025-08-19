# Core Feature: Synchronization

## 1. Summary

This document outlines the synchronization feature of the Automated Dotfile Manager. Synchronization is the process of keeping the dotfiles consistent across multiple machines. This is a key feature that leverages the version control integration to provide a seamless experience for users who work on more than one computer.

## 2. Intended Functionality

The synchronization process will involve:
- **Fetching remote changes:** Downloading the latest version of the dotfiles from a central remote repository.
- **Applying remote changes:** Updating the local dotfiles and repository to match the remote version. This includes updating symlinks if necessary.
- **Pushing local changes:** Uploading any local modifications to the dotfiles to the remote repository.
- **Conflict resolution:** A mechanism to handle cases where a dotfile has been changed both locally and remotely since the last sync.

The primary interface for this will be a single, simple command, such as `dotman sync`.

## 3. User Stories

- *As a user,* after setting up a new machine, I want to run `dotman sync` to pull all my dotfiles from my central repository and have them symlinked into place, so my new machine is configured just like my old one.
- *As a developer,* when I make a change to my `.vimrc` on my work machine, I want to run `dotman sync` to push that change to the remote. Then, when I get home, I can run `dotman sync` on my personal machine to get the updated `.vimrc`.
- *As a user,* if I have made local changes and there are also remote changes, I want the `sync` command to intelligently merge them if possible, or notify me if there is a conflict that I need to resolve manually.
- *As a user,* I want to be able to see the status of my local files compared to the remote before I sync, so I know what changes will be pulled or pushed.

## 4. Requirements

### Functional Requirements

- The `sync` command must perform a `pull` from the remote repository, followed by a `push` of any local changes.
- The tool must be able to handle different states of the local and remote repositories:
  - Local is behind remote: Fast-forward merge should be applied.
  - Local is ahead of remote: Local commits should be pushed.
  - Local and remote have diverged: A merge or rebase should be attempted.
- The tool must provide clear output about the synchronization process, including which files were updated, added, or deleted.
- In case of a merge conflict, the `sync` process should be halted, and the user should be provided with instructions on how to resolve the conflict. The tool should not leave the repository in a broken state.
- After a successful pull, the tool must ensure that the symlinks in the home directory are pointing to the correct, updated files in the repository.

### Non-Functional Requirements

- **Reliability:** The synchronization process must be robust, especially when handling network errors or merge conflicts. Data loss must be prevented.
- **Performance:** For typical usage, the `sync` command should complete within a few seconds, depending on network speed.
- **Usability:** The process should be as automatic as possible, requiring user intervention only when absolutely necessary (e.g., for complex merge conflicts).

## 5. Dependencies

### Internal Dependencies

- **Version Control Integration:** This feature is essentially a user-friendly wrapper around `git pull` and `git push`. It is entirely dependent on the Git integration.
- **CLI:** The `sync` command is part of the CLI.
- **Discovery and Tracking:** The sync process might need to update the manifest of tracked files if new files were added from a remote source.

### External Dependencies

- **Git:** The `git` command-line tool must be available.
- An active internet connection is required to sync with a remote repository.

## 6. Limitations & Assumptions

- This feature assumes the user is using a remote repository (like GitHub) as the central "source of truth" for their dotfiles. It does not cover peer-to-peer synchronization.
- The conflict resolution provided by the tool will be basic. For complex conflicts, the user will be expected to use standard Git tools.
- The tool will not manage SSH keys or other authentication mechanisms for Git. It assumes the user has already configured `git` to authenticate with the remote.
- It is assumed that the user has a single remote named `origin` for their dotfiles repository. The ability to manage multiple remotes could be an advanced feature.

## 7. Implementation Ideas

- The `dotman sync` command can be implemented as a script that executes the following Git commands:
  1. `git fetch origin`: To get the latest metadata from the remote.
  2. `git status -uno`: To check the status of the local repository compared to the remote. The output can be parsed to inform the user.
  3. `git pull --rebase`: To pull remote changes and replay local commits on top. This is generally cleaner for dotfiles than creating merge commits.
  4. `git push origin HEAD`: To push the (potentially rebased) local commits to the remote.
- Before syncing, the tool can check for local uncommitted changes and either ask the user to commit them first or automatically stash them.
- In case of a rebase conflict, the tool would exit with a clear error message, instructing the user to resolve the conflicts and then run `git rebase --continue`. The dotfile manager could have a `dotman sync --continue` command to simplify this.
- After a successful pull, the tool should verify that all tracked files have corresponding symlinks and that they are not broken. It could run a "health check" on the symlinks.
