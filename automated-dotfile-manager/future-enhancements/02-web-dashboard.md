# Future Enhancement: Web-Based Dashboard

## 1. Summary

This document proposes a web-based dashboard for the Automated Dotfile Manager. This would be a hosted service or a self-hostable web application that allows users to manage their dotfiles from any web browser. This complements the CLI and potential GUI by providing remote access and a centralized overview of all managed machines.

## 2. Intended Functionality

The web dashboard would offer:
- **Centralized Management:** A single place to view all your dotfiles, across all your machines.
- **Remote Editing:** An in-browser editor to make changes to dotfiles without having to SSH into a machine. Changes would be committed to the Git repository.
- **Machine Status Overview:** A dashboard showing a list of all machines linked to the account, their last sync time, and whether they are up-to-date.
- **Visual History and Rollback:** A web-based `git log` view, allowing users to browse the history of their dotfiles and trigger a rollback to a previous version.
- **Sharing and Collaboration:** The ability to share dotfile profiles with other users or teams.
- **Onboarding:** A web-based onboarding flow for new users, helping them create their dotfiles repository and providing instructions for installing the client on their machines.

## 3. User Stories

- *As a user,* I'm on a friend's computer and I want to quickly edit a typo in my `.vimrc`. I want to log into a web dashboard, make the change in a web editor, and save it, so it will be synced to my machines later.
- *As a team lead,* I want to manage a standard set of dotfiles for my development team. I want to use a web dashboard to update these files and see which team members have synced the latest version.
- *As a user with many machines,* I want a single dashboard where I can see the status of all of them at a glance, to make sure they are all in sync.
- *As a content creator,* I want to share a link to my dotfile setup for my followers. A public, read-only view of my dotfiles on the web dashboard would be perfect.

## 4. Requirements

### Functional Requirements

- User authentication and authorization system.
- Integration with Git hosting providers (GitHub, GitLab) for repository access.
- A web-based text editor (like Monaco, the editor that powers VS Code).
- A backend service to interact with the user's Git repository.
- A frontend application to display the dashboard, editor, and machine status.
- An API for the client application (`dotman`) to report its status to the dashboard.

### Non-Functional Requirements

- **Security:** This is paramount. The service would have access to users' Git repositories and potentially sensitive information. It must be secure against common web vulnerabilities (XSS, CSRF, etc.).
- **Scalability:** The service should be able to handle many users and frequent status updates from their clients.
- **Privacy:** User data and dotfiles must be kept private and secure. The privacy policy should be clear.
- **Reliability:** The service needs to be highly available.

## 5. Dependencies

### Internal Dependencies

- The web dashboard would require a well-defined **API** in the core application for the client to communicate with it.

### External Dependencies

- **Cloud Infrastructure:** A hosting provider (like AWS, Google Cloud, or Vercel).
- **Database:** To store user accounts and machine information.
- **Web Framework:** For both backend (e.g., Django, Ruby on Rails, Express.js) and frontend (e.g., React, Vue, Svelte).
- **Git Hosting Provider APIs:** To authenticate users and interact with their repositories.

## 6. Limitations & Assumptions

- This is a very large undertaking, turning the project from a local tool into a full-fledged SaaS product.
- A self-hosted option should also be considered for users who do not want to use a third-party service.
- The business model (free, freemium, paid) would need to be defined.
- This assumes that users are willing to grant a web service access to their dotfiles repository.

## 7. Implementation Ideas

- The architecture would likely be a standard monolithic or microservices-based web application.
- **Authentication:** OAuth with GitHub/GitLab would be the most convenient way for users to sign up and grant repository access.
- **Backend:** The backend would be responsible for all Git operations. It would maintain a clone of the user's dotfiles repository and perform operations on it on behalf of the user. Webhooks could be used to keep the clone up-to-date.
- **Frontend:** A modern single-page application (SPA) framework like React or Vue would be suitable for building the interactive dashboard.
- **Client-Service Communication:** The `dotman` client on the user's machine would need an API key to authenticate with the service. On every `sync`, it would send a small payload to the backend API with the machine's hostname, the current git hash, and a timestamp.
- **Self-Hosting:** The application could be packaged as a Docker container to make self-hosting easier. A `docker-compose.yml` file could define the entire stack (backend, frontend, database).
- For the in-browser editor, integrating a pre-built component like Monaco Editor would provide a rich editing experience with syntax highlighting.
