# Feature: Profiles and Contexts

## Description

This advanced feature allows users to manage multiple, distinct sets of dotfiles for different machines, operating systems, or contexts (e.g., a "work" machine vs. a "personal" machine).

## Intended Functionality

- **Profile Definition**: Users can define different profiles in the main configuration file. Each profile will specify a set of dotfiles to be managed.
- **Context-specific Configuration**: Profiles can be used to apply different versions of a dotfile based on the context. For example, a `.gitconfig` might have a work email for the "work" profile and a personal email for the "personal" profile.
- **Profile Switching**: A command or flag to specify which profile to use when running a command (e.g., `dotfile-manager install --profile=work`). The current profile could be stored in a local config file so it doesn't have to be specified every time.
- **Layered Configurations**: Support for a base profile that is inherited and extended by other profiles. For example, a `base` profile could contain common dotfiles like `.bashrc`, and a `linux` profile could add `.Xresources`, while a `macos` profile adds `.hammerspoon` config. When on a Linux work machine, the user could apply both the `linux` and `work` profiles.
- **Hostname Detection**: The tool could automatically detect the hostname of the machine and apply the corresponding profile if it exists.

## Requirements

- The configuration file format needs to be able to handle this complexity. A simple list of files will no longer be sufficient. TOML or YAML would be good candidates.
- A clear and intuitive way to define and manage profiles in the configuration.

## Limitations

- This feature can add significant complexity. The user interface for managing profiles must be designed carefully to avoid confusion.
- Overlapping profiles could lead to conflicts that need to be resolved.

## Dependencies

- **`core/dotfile-tracking.md`**: This feature extends the basic concept of tracking files by allowing different sets of files to be tracked.
