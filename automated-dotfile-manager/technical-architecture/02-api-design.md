# Technical Architecture: API Design

## 1. Summary

This document outlines the internal API design for the Automated Dotfile Manager. This is not a web API, but rather the programmatic interface of the core logic, which will be written as a library or a set of modules. A clean, well-defined internal API is crucial for long-term maintainability and for enabling different "heads" for the application, such as the CLI, a GUI, or a web service.

## 2. Guiding Principles

- **Decoupling:** The core logic should be completely independent of the user interface (CLI, GUI). The core library should not print to the console or display dialog boxes. It should return data and errors.
- **Statelessness:** The core API functions should be as stateless as possible. They should operate on a "context" or "repository" object that holds all the necessary state.
- **Clear Error Handling:** Functions should return explicit errors that can be inspected by the caller to provide appropriate feedback to the user.
- **Modularity:** The API should be organized into logical modules that correspond to the main features of the application (e.g., `tracking`, `sync`, `profiles`).

## 3. Core `DotfileManager` Object

The main entry point to the API will be a `DotfileManager` object (or struct). This object will be instantiated by the client (e.g., the CLI) and will hold the context for all operations.

**Initialization:**
`NewDotfileManager(configPath: string) -> (DotfileManager, error)`
- Loads the global and repository-level configuration.
- Initializes the storage backend (e.g., the Git backend).
- Returns a configured `DotfileManager` instance.

**Properties:**
- `Config`: The loaded configuration.
- `Repository`: An object representing the dotfiles repository.
- `Backend`: The storage backend interface.

## 4. API Modules and Key Functions

### 4.1. Tracking Module (`tracking`)

Handles adding, removing, and listing tracked files.

- `ListTrackedFiles() -> ([]TrackedFile, error)`
  - Reads the manifest and returns a list of all tracked file objects.
- `AddFile(filePath: string, profile: string, isTemplate: bool, isSecret: bool) -> (error)`
  - Manages the entire process of adding a file: updates the manifest, moves the file, creates the symlink, and commits the changes via the backend.
- `RemoveFile(filePath: string) -> (error)`
  - Removes a file from tracking: updates the manifest, removes the symlink and the file from the repository, and commits the changes.
- `GetFileStatus(filePath: string) -> (FileStatus, error)`
  - Checks the status of a single file (e.g., modified, untracked, synced).

### 4.2. Synchronization Module (`sync`)

Handles synchronization with the remote.

- `Sync(progressCallback: func) -> (SyncResult, error)`
  - Orchestrates the entire sync process.
  - Pulls from the remote backend.
  - Checks for necessary updates (template rendering, secret decryption).
  - Pushes local changes to the remote.
  - The `progressCallback` can be used by the UI to report progress.
  - `SyncResult` would contain information about the sync, like number of files updated.
- `GetRemoteStatus() -> (RemoteStatus, error)`
  - Compares the local and remote state to determine if the local repo is ahead, behind, or has diverged.

### 4.3. Profiles Module (`profiles`)

Manages profiles.

- `ListProfiles() -> ([]Profile, error)`
  - Returns a list of all available profiles defined in the configuration.
- `GetActiveProfile() -> (Profile, error)`
  - Reads the local state file to determine the active profile.
- `ActivateProfile(profileName: string) -> (error)`
  - Sets the new active profile in the local state.
  - Orchestrates the process of unlinking old files and linking new ones based on the manifest.

### 4.4. Templating Module (`templating`)

- `RenderAllTemplates() -> (error)`
  - Finds all template files in the repository.
  - Gathers the necessary variables from the configuration based on the active profile.
  - Renders each template to its destination file.

### 4.5. Secrets Module (`secrets`)

- `EncryptFile(filePath: string) -> (error)`
  - Encrypts a single file using the configured secret backend (e.g., GPG).
- `DecryptFile(encryptedPath: string) -> (error)`
  - Decrypts a single file.
- `EditSecret(filePath: string) -> (error)`
  - A helper function that orchestrates the decrypt -> open in editor -> re-encrypt workflow.

## 5. Example Usage (from a hypothetical CLI)

```go
// main.go (for the CLI)

func main() {
    // 1. Initialize the core manager
    manager, err := core.NewDotfileManager("~/.config/dotman/config.toml")
    if err != nil {
        // handle error
    }

    // Example: Implementing the "list" command
    if os.Args[1] == "list" {
        files, err := manager.Tracking.ListTrackedFiles()
        if err != nil {
            // print error to user
        }
        // Iterate through files and print them
    }

    // Example: Implementing the "sync" command
    if os.Args[1] == "sync" {
        progressChan := make(chan string)
        go func() {
            // UI goroutine to print progress messages
            for msg := range progressChan {
                fmt.Println(msg)
            }
        }()

        // The progress callback sends messages to the channel
        callback := func(msg string) {
            progressChan <- msg
        }

        result, err := manager.Sync.Sync(callback)
        if err != nil {
            // print error to user
        }
        close(progressChan)
        // print final result
    }
}
```

This design ensures that the complex logic is contained within the `core` library, and the CLI (`main.go`) is just a thin wrapper that calls the API and handles user-facing input and output. A GUI application would do the same, calling the same `core` functions but presenting the results and errors in a graphical way.
