# Privacy Policy — Insait Edit Code Editor

*Last updated: August 4, 2026*

This Privacy Policy explains what data the **Insait Edit Code Editor** application (hereinafter the "App") processes, and how it is stored and protected.

The App was built with privacy as a priority: it contains no advertising, collects no analytics, and does not share any data with third-party trackers. All data processing happens locally on your device, except for interaction with GitHub, which you consciously initiate.

---

## 1. Data stored locally

The App only stores the data needed for it to work, in the AppData folder of your system:

| Data | Where it is stored | Description |
|------|--------------------|-------------|
| App settings | `%AppData%\Insait Edit Code Editor` | Interface language and other settings, in an encrypted database (LiteDB + DPAPI). |
| GitHub token | `%AppData%\Insait Edit Code Editor` | GitHub access token, stored in an encrypted database (LiteDB + DPAPI). |
| List of recent projects | `%AppData%\Insait Edit Code Editor` | Paths to recently opened files and folders. |
| Local translations | `%AppData%\Insait Edit Code Editor\GitHubTranslations` | Additional interface files for localization. |
| Logs | `%LocalAppData%\Insait Edit Code Editor\Logs` | Service logs for troubleshooting. |

The encrypted databases (LiteDB + DPAPI) are protected so that only your Windows user account can decrypt them.

The App does **not** read, collect, or transmit the contents of your files, scripts, or code. Your files are opened only upon your explicit command and are processed locally.

---

## 2. GitHub integration

To work with Git and GitHub (cloning repositories, creating commits, publishing), the App may interact with the GitHub API.

- **Authentication** is performed via the GitHub OAuth Device Flow or a personal token that you enter yourself.
- The obtained token is stored **only locally** in the encrypted database (see section 1) and is **not** sent to any third parties.
- The App accesses the GitHub API **only at your command** (load a repository, check status, create a commit, etc.) and only on your behalf.
- Information about your GitHub account (login, name, email, avatar) is used solely for display within the App itself.

When using GitHub features, the [GitHub Privacy Statement](https://docs.github.com/en/site-policy/privacy-policies/github-privacy-statement) applies. The list of repositories and their contents that you open does not leave your device.

---

## 3. Disclosure of data to third parties

The App does **not** transfer your personal data to advertising, analytics, or other third-party services. The exceptions are:

- **GitHub** — only upon your explicit action (authentication, cloning, commits, publishing).
- **Git** (a local tool) — works locally; network operations (push/pull/clone) are performed only when you initiate them and only with repositories you specify.

---

## 4. Logs and diagnostics

The App may write service logs to `%LocalAppData%\Insait Edit Code Editor\Logs`. They are intended for troubleshooting and **do not** contain your code content, passwords, or secrets. Logs are stored locally and are never sent anywhere automatically.

---

## 5. Children

The App is not directed at children and does not knowingly collect personal data from children under the age of 13.

---

## 6. Your rights

You can at any time:

- delete the stored GitHub token (via the sign-out option in the App);
- completely remove all local data by deleting the folders `%AppData%\Insait Edit Code Editor` and `%LocalAppData%\Insait Edit Code Editor`;
- use the App without signing in to GitHub — most features (editing, running scripts, learning) work fully offline.

---

## 7. Changes to this Policy

We may update this Privacy Policy from time to time. The current version is always available together with the App. By continuing to use the App after changes, you agree to the updated Policy.

---

## 8. Contact

If you have questions about this Privacy Policy, please contact us via the App's page in the Microsoft Store.

---

*Document prepared for the Microsoft Store submission.*