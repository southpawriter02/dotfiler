# Future Enhancement: Plugin System

## 1. Summary

This document proposes a plugin system for the Automated Dotfile Manager. A plugin system would allow the community to extend the core functionality of the tool without modifying the core codebase. This could lead to a rich ecosystem of new features, integrations, and support for a wider range of tools and workflows.

## 2. Intended Functionality

A plugin system would allow plugins to:
- **Add new CLI commands:** A plugin could register new subcommands under the main `dotman` command (e.g., `dotman myplugin subcommand`).
- **Hook into existing events:** Plugins could execute code at specific points in the application's lifecycle, such as `pre_sync`, `post_sync`, `on_add_file`, etc.
- **Add support for new package managers:** The dependency management system could be made extensible, allowing plugins to add support for less common package managers.
- **Add support for new secret backends:** The secret management system could allow plugins to add support for different secret stores like HashiCorp Vault or 1Password.
- **Add new template functions:** The templating engine could be extended with custom functions provided by plugins.

## 3. User Stories

- *As a developer,* I want to write a plugin that automatically formats my shell scripts with `shfmt` every time I sync my dotfiles, so I can enforce a consistent style.
- *As a user of the Nix package manager,* the core tool doesn't support it. I want to be able to install a `dotman-nix` plugin that adds support for checking and installing dependencies with Nix.
- *As a security engineer,* I want to write a plugin that integrates with my company's internal key management service for handling secrets, so I can use the dotfile manager securely within my corporate environment.
- *As a power user,* I want to create a plugin that adds a `dotman lint` command, which runs a suite of linters against my configuration files.

## 4. Requirements

### Functional Requirements

- A clear and stable API for plugins must be defined.
- The tool needs a mechanism to discover and load plugins (e.g., from a specific directory).
- The plugin API should provide access to the core components of the application, such as the configuration, the list of tracked files, and the repository object.
- The tool must provide a way for users to install and manage plugins.
- Plugins must be sandboxed to some extent to prevent them from breaking the core application. A misbehaving plugin should not crash the main tool.

### Non-Functional Requirements

- **Security:** Plugins execute arbitrary code. The system must be secure. Perhaps a permission system for plugins could be implemented, where users are warned about what capabilities a plugin requires.
- **Performance:** Loading many plugins should not significantly slow down the startup time of the CLI.
- **Stability:** The plugin API must be versioned and kept stable. Breaking changes to the API should be minimized.
- **Documentation:** The plugin API must be thoroughly documented to encourage community contributions.

## 5. Dependencies

### Internal Dependencies

- This feature would require a significant refactoring of the core application to be more modular and expose a clean internal API. The application logic would need to be designed with extensibility in mind from the start.

### External Dependencies

- Depending on the implementation, it might require libraries for dynamic code loading or inter-process communication.

## 6. Limitations & Assumptions

- A plugin system is a very advanced feature and adds a lot of complexity to the application.
- Security is a major concern. A poorly written plugin could be a security risk.
- Maintaining a stable plugin API is a significant long-term commitment.

## 7. Implementation Ideas

There are several common models for plugin architectures:
- **Dynamic Loading (e.g., Python entry points, Go plugins):** The main application can dynamically load shared libraries (`.so`, `.dll`) or modules at runtime. This is powerful but can be complex and have portability issues.
- **External Scripts/Executables:** This is a simpler and more secure model. The main tool calls external scripts or executables following a specific naming convention (e.g., `dotman-nix`). The `git` command itself uses this model. The tool would discover these executables in the user's `PATH`. Communication can be done via `stdin`/`stdout` and environment variables.
- **In-process scripting (e.g., Lua, JavaScript):** The application can embed a scripting language interpreter. Plugins are then written in that language. This provides a sandboxed environment but requires an interpreter to be bundled with the tool.

**The External Script model is often the best choice for a CLI tool:**
- **Implementation:**
  - `dotman` would scan the `PATH` for executables named `dotman-*`.
  - When a user runs `dotman foo bar`, if `foo` is not a built-in command, the tool would look for an executable named `dotman-foo` and execute it with the arguments `bar`.
  - For hooks, the main tool could execute a script like `~/.config/dotman/hooks/post-sync` if it exists.
  - The main tool would pass information to the plugin via environment variables (e.g., `DOTMAN_REPO_PATH`, `DOTMAN_ACTIVE_PROFILE`) and/or a JSON payload via `stdin`.
- **Plugin Management:**
  - A command like `dotman plugin install <url>` could simply be a helper that downloads a script or binary and puts it into a specific directory in the `PATH` (e.g., `~/.local/bin/dotman-nix`).
- This approach is language-agnostic (plugins can be written in any language), secure (plugins run in their own process), and simple to implement and understand. It's a proven model used by `git`, `kubectl`, and many other successful tools.
