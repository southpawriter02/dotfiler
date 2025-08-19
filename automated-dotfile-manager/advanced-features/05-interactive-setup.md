# Advanced Feature: Interactive Setup

## 1. Summary

This document describes an interactive setup feature for the Automated Dotfile Manager. The goal of this feature is to provide a guided, user-friendly experience for new users setting up the dotfile manager for the first time, or for users setting up their dotfiles on a new machine. This "wizard"-style interface would be an alternative to manually running a series of CLI commands.

## 2. Intended Functionality

The interactive setup process would guide the user through:
- **Initialization:** Choosing a directory for the dotfiles repository.
- **Discovery:** Scanning the home directory for potential dotfiles and letting the user select which ones to track.
- **Remote configuration:** Asking for a remote Git repository URL for backup and synchronization.
- **Initial commit and push:** Performing the first commit and push to the remote repository.
- **Dependency check:** Running an initial check for dependencies and asking the user if they want to install them.

## 3. User Stories

- *As a new user,* I want to run `dotman init --interactive` and be guided through the entire process of setting up my dotfile management, from choosing files to pushing them to GitHub.
- *As a user setting up a new machine,* I want an interactive command that asks me which profile to activate, checks for missing dependencies, and offers to install them, making the new machine setup process smooth and foolproof.
- *As a non-expert user,* I find the list of commands overwhelming. I would prefer a single command that asks me questions in plain English to get my dotfiles set up.
- *As an instructor,* I want to tell my students to use the interactive setup to get their development environment configured, as it reduces the chance of errors.

## 4. Requirements

### Functional Requirements

- An interactive mode must be triggered by a specific command or flag, e.g., `dotman init --interactive` or `dotman setup`.
- The interactive session must be a step-by-step process.
- It must be able to:
  - Prompt the user for input (e.g., repository path).
  - Present the user with a list of choices (e.g., a multi-select list of dotfiles to track).
  - Ask for yes/no confirmation for actions (e.g., "Push to remote repository? [y/N]").
- The process should be pausable or cancellable at any step.
- At the end of the process, the dotfile manager should be in a fully configured and operational state.
- The interactive setup for a new machine should be able to pull an existing repository and guide through profile selection and dependency installation.

### Non-Functional Requirements

- **Usability:** This is the key requirement. The interface must be clear, intuitive, and easy to navigate. The questions should be unambiguous.
- **Robustness:** The interactive session should handle invalid user input gracefully (e.g., if a user enters an invalid path).
- **Discoverability:** The tool's help text should clearly advertise the existence of the interactive mode.

## 5. Dependencies

### Internal Dependencies

- This feature is a high-level wrapper that orchestrates most of the other core and advanced features:
  - **CLI:** It is invoked from the CLI.
  - **Discovery and Tracking:** It uses the discovery mechanism to suggest files.
  - **Version Control Integration:** It initializes the Git repository and performs commits/pushes.
  - **Configuration Management:** It creates and populates the configuration file.
  - **Dependency Management:** It uses the dependency checker and installer.

### External Dependencies

- A library for building interactive command-line prompts is essential. Examples include:
  - **Inquirer.js** (for Node.js)
  - **prompt-toolkit** (for Python)
  - **Bubble Tea** (for Go)
  - **dialog** or **whiptail** (classic shell script utilities)

## 6. Limitations & Assumptions

- This feature is not intended to cover all possible configurations and edge cases. It is for the most common setup workflows.
- For complex or highly customized setups, the user will still need to use the standard CLI commands and edit configuration files manually.
- It is assumed that the user is running the tool in an interactive terminal session that can support features like cursor movement and colored text.

## 7. Implementation Ideas

- The `dotman setup` command would be the entry point.
- The implementation would be a state machine, where each state represents a question or an action.
- A library like Bubble Tea (Go) or Inquirer.js (Node) would be used to build the TUI (Text-based User Interface).
- The process could look like this:
  1. **Welcome screen:** "Welcome to the Automated Dotfile Manager setup!"
  2. **Choose mode:** "Are you setting up for the first time, or syncing an existing setup to a new machine?"
  3. **(First time):**
     - Ask for repository path.
     - Scan for dotfiles and present a checklist.
     - Ask user to confirm the list of files to track.
     - Add the selected files.
     - Ask for remote URL.
     - Perform initial commit and push.
     - Ask to check for dependencies.
  4. **(New machine):**
     - Ask for remote URL.
     - Clone the repository.
     - If profiles are used, list available profiles and ask the user to activate one.
     - Run dependency check and ask to install missing ones.
  5. **Completion screen:** "Setup complete! You can now use `dotman sync` to keep your files up to date."
- Each step would call the underlying API of the other modules (e.g., the tracking module's `add_file` function). This ensures that the core logic is not duplicated. The interactive setup is just another "view" on top of the core application logic.
