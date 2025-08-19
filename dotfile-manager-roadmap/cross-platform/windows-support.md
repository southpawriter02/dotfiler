# Feature: Windows Support

## Description

This document outlines the considerations and a plan for making the automated dotfile manager fully functional on the Windows operating system. This is a significant undertaking due to the fundamental differences between Windows and POSIX-like systems (Linux, macOS).

## Intended Functionality

- **Path Handling**: The application must be able to handle Windows-style file paths (`C:\Users\Jules\`) as well as POSIX-style paths (`/home/Jules/`). The code should use a platform-agnostic way of joining and manipulating paths (e.g., Python's `os.path.join`).
- **Symbolic Links**: Symbolic link creation on Windows is more restrictive than on POSIX systems.
    - It often requires administrator privileges.
    - The tool should detect if it has the necessary permissions and inform the user.
    - Alternatives like hard links or NTFS junctions could be explored, but they have different semantics.
    - For users on modern versions of Windows 10/11 with Developer Mode enabled, creating symlinks is easier. The tool should detect this and use it if available.
- **Dotfile Locations**: On Windows, configuration files are not typically stored in the user's home directory with a leading dot. Instead, they are often located in `%APPDATA%`, `%LOCALAPPDATA%`, or `%USERPROFILE%`. The configuration format must be able to specify the correct target location for files on a per-OS basis. The `profiles` feature can be leveraged for this.
- **Shells**: The tool should not assume a POSIX-compliant shell like Bash is available.
    - Any internal shell commands should be compatible with both `cmd.exe` and `PowerShell`.
    - For the `scripting` feature, the user should be able to specify the shell to use (e.g., `powershell`, `cmd`, `bash`). The documentation should clearly state that Git for Windows (which includes Git Bash) is a recommended dependency for users who want to run bash scripts.

## Requirements

- A Windows development and testing environment.
- Continuous Integration (CI) that runs tests on a Windows runner.
- Careful abstraction of filesystem operations.

## Limitations

- Achieving perfect feature parity might be difficult. Some features, especially those relying on deep POSIX integration, might be limited on Windows.
- The user experience for creating symlinks might be more complex on Windows.

## Dependencies

- This is a cross-cutting concern that affects almost every other feature, especially:
    - **`core/initial-setup.md`**
    - **`core/synchronization.md`** (specifically symlinking)
    - **`advanced/profiles-and-contexts.md`** (for OS-specific configurations)
    - **`advanced/scripting-and-automation.md`** (for shell compatibility)
