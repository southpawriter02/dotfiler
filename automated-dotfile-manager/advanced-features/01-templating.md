# Advanced Feature: Templating Engine

## 1. Summary

This document describes the templating engine for the Automated Dotfile Manager. This feature allows users to create dynamic dotfiles that are customized for different machines or environments from a single source template. For example, a `.gitconfig` file could use a different email address on a work machine versus a personal machine, without having to maintain two separate files.

## 2. Intended Functionality

The templating engine will process template files and generate the final dotfiles from them. Its key functionalities are:
- **Template processing:** The tool will identify files that are templates (e.g., by a `.tpl` extension) and process them to generate the output dotfile.
- **Variable substitution:** The engine will substitute variables defined in a configuration file into the templates.
- **Conditional blocks:** The templates will support conditional logic (e.g., `if/else` blocks) to include or exclude parts of the file based on variables.
- **Machine-specific values:** The system will allow users to define variables that are specific to each machine, which the templating engine will then use.

## 3. User Stories

- *As a consultant,* I want to use the same `.gitconfig` for all my clients, but have my email address change depending on which machine I'm on. I want to define a `user.email` variable for each machine and have my `.gitconfig.tpl` use it.
- *As a developer,* I have a powerful workstation and a lightweight laptop. I want to use a template for my shell configuration that includes performance-heavy aliases and functions only on the workstation.
- *As a user,* I want to be able to define a set of common variables once (e.g., my name) and have them available in all my templates, so I don't have to repeat myself.
- *As a user,* when I run `dotman sync`, I want the tool to automatically re-generate any dotfiles whose templates or variables have changed.

## 4. Requirements

### Functional Requirements

- The tool must be able to process template files and generate output files.
- The templating language should support:
  - Variable substitution (e.g., `{{ .variable }}`).
  - Conditional blocks (`if/else/end`).
  - Loops (for iterating over lists of variables).
- The tool must provide a way for users to define variables. This could be in the main configuration file or in separate, machine-specific files.
- The tool should automatically re-run the template generation process when a template file is modified or when the variables used in it are changed.
- The generated files (the actual dotfiles) should be the ones that are symlinked into the user's home directory. The template files themselves should remain in the managed repository.

### Non-Functional Requirements

- **Performance:** The templating engine should be fast enough that it doesn't noticeably slow down the `sync` process.
- **Usability:** The templating syntax should be easy to learn and use. It should be a well-known and documented syntax.
- **Security:** The templating engine must not allow arbitrary code execution. It should be limited to variable substitution and simple logic.

## 5. Dependencies

### Internal Dependencies

- **CLI:** A command might be provided to manually re-generate all templates, e.g., `dotman template render`.
- **Configuration Management:** The variables for the templates will be stored in the configuration files.
- **Synchronization:** The `sync` command should trigger the templating engine.

### External Dependencies

- A templating library. Popular choices include:
  - **Jinja2** (for Python)
  - **Go's `text/template`** (for Go)
  - **Handlebars** or **Liquid** (for many languages)

## 6. Limitations & Assumptions

- This feature adds a layer of abstraction that might be confusing for new users. The documentation must be very clear.
- The choice of templating language will lock the user into a specific syntax.
- The initial implementation might not support very advanced templating features like macros or template inheritance.
- It is assumed that the user is comfortable with the concept of templates and variables.

## 7. Implementation Ideas

- A popular and safe templating engine like Go's `text/template` or `html/template` would be a good choice. It provides the necessary features without being overly complex or insecure.
- The tool could look for files with a specific extension, like `.dotfile.tpl`, and automatically know to process them. For example, `.gitconfig.tpl` would generate `.gitconfig`.
- Variables could be defined in a `variables` section of the `config.toml` file. For machine-specific values, the user could create a file named `config.<hostname>.toml`, and the tool would load it if it exists.
- The `sync` process would be modified:
  1. Pull changes from remote.
  2. Check if any template files (`.tpl`) or variable definitions have changed.
  3. If so, re-render all affected templates to generate the output dotfiles.
  4. The generated files are now part of the repository's working directory, and their changes can be committed and pushed.
- An example template for `.gitconfig.tpl`:
  ```
  [user]
      name = {{ .user.name }}
      email = {{ .user.email }}

  [core]
      editor = vim

  {{ if eq .system "work" }}
  [includeIf "gitdir:~/work/"]
      path = .gitconfig-work
  {{ end }}
  ```
  And the corresponding `config.toml` variables:
  ```toml
  [variables]
  user.name = "Jules"
  user.email = "jules@personal.com"
  system = "personal" # This could be overridden on a work machine
  ```
