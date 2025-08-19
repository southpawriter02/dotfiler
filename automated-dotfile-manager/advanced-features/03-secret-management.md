# Advanced Feature: Secret Management

## 1. Summary

This document outlines a system for managing secrets within dotfiles. Users often have sensitive information, such as API tokens, passwords, or private keys, in their configuration files. Storing these directly in a public Git repository is a major security risk. This feature provides a mechanism to encrypt secrets before committing them to the repository and decrypt them on the fly when needed.

## 2. Intended Functionality

The secret management system will provide:
- **Encryption/Decryption:** A way to encrypt sensitive files or parts of files before they are stored in the Git repository. The tool will decrypt them when deploying the dotfiles on a machine.
- **Integration with VCS:** The system will be designed to work seamlessly with Git. Encrypted files will be committed to the repository, while the unencrypted "live" dotfiles will be git-ignored.
- **Key Management:** A secure way to manage the encryption keys. This might involve using GPG/PGP keys or other secure key management solutions.
- **Selective Encryption:** Users should be able to specify which files are secret and need to be encrypted.

## 3. User Stories

- *As a developer,* I want to store my `.env` file with my application secrets in my dotfiles repository, but I want it to be encrypted so that I can safely push it to a public GitHub repository.
- *As a user,* when I run `dotman sync` on a new machine, I want the tool to decrypt my secret files after pulling them from the remote, asking for my GPG passphrase once.
- *As a user,* I want to be able to edit an encrypted file, and have the tool automatically re-encrypt it when I'm done, so the process is as transparent as possible.
- *As a security-conscious user,* I want to be able to use my existing PGP key pair for encryption, so I don't have to manage a new set of keys.

## 4. Requirements

### Functional Requirements

- The tool must provide commands to encrypt and decrypt files.
- It must be possible to mark a file as "secret". When a secret file is added, it should be encrypted.
- The unencrypted version of the secret files must be added to the repository's `.gitignore` file to prevent accidental commits.
- The `sync` command must automatically decrypt any newly pulled or updated secret files.
- The system must support a robust and standard encryption method, such as GPG/PGP.
- The tool needs a way to identify which key to use for encryption/decryption.

### Non-Functional Requirements

- **Security:** This is the most critical requirement. The encryption must be strong, and the key management must be secure. Private keys should never be stored in the repository.
- **Usability:** The process of managing secrets should be as user-friendly as possible. It should not require deep knowledge of cryptography.
- **Portability:** The encryption/decryption mechanism should work across all supported operating systems.

## 5. Dependencies

### Internal Dependencies

- **CLI:** Commands like `dotman secret encrypt <file>` and `dotman secret edit <file>` will be provided.
- **Version Control Integration:** The system needs to carefully manage which files are committed and which are ignored.
- **Synchronization:** The `sync` command will trigger the decryption process.

### External Dependencies

- A cryptographic tool like **GnuPG (GPG)** must be installed on the system.
- Alternatively, a cryptographic library could be used (e.g., `age` by Filippo Valsorda, or a PGP library for the chosen programming language).

## 6. Limitations & Assumptions

- This feature assumes the user has a basic understanding of what encryption is and why it's necessary for secrets.
- The user is responsible for securely managing their private keys. If the private key is lost, the encrypted data will be unrecoverable.
- The initial implementation will likely focus on encrypting entire files, not specific values within a file (inline secrets).
- It is assumed that the user has GPG or another supported cryptographic tool installed and configured.

## 7. Implementation Ideas

- This feature can be inspired by tools like `git-crypt` or `sops`.
- When a user wants to track a secret file, say `~/.secret_token`, they would run `dotman secret add ~/.secret_token`.
- This command would:
  1. Add the path `~/.secret_token` to a special list of managed secrets.
  2. Add the pattern `.secret_token` to the `.gitignore` file in the repository.
  3. Encrypt the file `~/.secret_token` to `secret_token.gpg` inside the repository.
  4. The encrypted file, `secret_token.gpg`, is what gets committed to Git.
- The `dotman sync` command, after pulling from the remote, would check for any `.gpg` files. For each one, it would use GPG to decrypt it to its original location. It would only need to ask for the user's passphrase once per session.
- An "edit" command (`dotman secret edit .secret_token`) could be implemented to provide a seamless workflow:
  1. The tool decrypts the corresponding `.gpg` file to a temporary location.
  2. It opens the temporary file in the user's default editor (`$EDITOR`).
  3. When the editor exits, the tool re-encrypts the temporary file back to its `.gpg` location and deletes the temporary file.
- The choice of which GPG key to use could be configured in the `config.toml` file. The user would provide their GPG key ID. For multi-user setups, it could support encrypting for multiple recipients.
- Using a tool like `age` could be simpler than GPG, as it has a more modern and straightforward CLI and key format. The principles would be the same.
