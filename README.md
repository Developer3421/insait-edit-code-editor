<p align="center">
  <img src="Insait Edit Code Editor/Icons/AppIcon.png" alt="Insait Edit Code Editor" width="128" height="128"/>
</p>

<h1 align="center">Insait Edit Code Editor</h1>

<p align="center">
  <b>A full-cycle Windows desktop IDE for building, running, publishing, and packaging .NET applications.</b>
</p>

<p align="center">
  <img alt=".NET 10" src="https://img.shields.io/badge/.NET-10.0-512BD4?logo=dotnet&logoColor=white"/>
  <img alt="Avalonia UI" src="https://img.shields.io/badge/Avalonia_UI-12.0-8B44AC?logo=avalonia&logoColor=white"/>
  <img alt="Roslyn" src="https://img.shields.io/badge/Roslyn-5.0-blue"/>
  <img alt="Platform" src="https://img.shields.io/badge/Platform-Windows-0078D6?logo=windows&logoColor=white"/>
  <img alt="License" src="https://img.shields.io/badge/License-MIT%20(with%20exclusions)-yellow"/>
</p>

---

## Overview

**Insait Edit Code Editor** is a Windows desktop IDE focused on C# and .NET development. It combines a custom code editor, Roslyn-powered code intelligence, project templates, build and publish workflows, Git integration, a built-in terminal, and MSIX packaging tools in a single application.

The IDE includes project creation for **Avalonia**, **Windows Forms**, **WPF**-compatible workflows, **console apps**, **class libraries**, and **F#** starter projects.

## Screenshots

<p align="center">
  <img src="Insait Edit Code Editor/Screenshots/English1.png" alt="Insait Edit Code Editor - main window" width="900"/>
</p>

<p align="center">
  <img src="Insait Edit Code Editor/Screenshots/English2.png" alt="Insait Edit Code Editor - code editor" width="900"/>
</p>

## Features

### Editor and code intelligence

- Custom `Insait Code Editor` with syntax highlighting and an editor surface built for desktop development
- Roslyn-backed C# completion, signature help, quick info, navigation, rename, and refactoring
- Inline diagnostics with quick fixes and auto-fix workflows
- F# completion and tooltip support through `FSharp.Compiler.Service`
- AXAML/XAML completion for Avalonia controls, properties, events, attached properties, and markup extensions
- AXAML preview window and live preview tooling

### Project and solution workflows

- Create solutions and projects directly from the IDE
- Add projects and files to an existing solution
- Work with C#, F#, AXAML, XAML, images, and project assets
- Build, rebuild, run, stop, publish, and package desktop applications
- Run configurations and compound run support

### Build, publish, and deployment

- MSBuild and `dotnet` integration
- Publish window with visual progress reporting
- NuGet panel for installing, updating, and removing packages
- MSIX manager for packaging, metadata editing, icon replacement, and signing

### Source control and terminal

- Git panel with status, staged and unstaged changes, branches, history, commit, push, and pull
- Repository cloning workflows
- Built-in terminal panel with process control and command history
- File explorer, search in files, image viewer, notifications, encoding tools, and project property windows

### Localization

- Built-in interface languages: **English, Ukrainian, German, Russian, and Turkish**
- Custom AXAML language dictionaries loaded at runtime
- Import of external `.axaml` translation files from the language menu

## System Requirements

- **Windows 10 / 11**
- **.NET 10 SDK** installed and available on `PATH`
- **Git for Windows** installed and available on `PATH` for source control, repository cloning, and Copilot CLI features
- **Windows SDK** (optional) for MSIX packaging, metadata signing, and deployment

> The application resolves some internal paths from the system drive. Running Windows and the project workspace from the `C:` drive is recommended.

## Getting Started

```powershell
git clone https://github.com/Developer3421/insait-edit-code-editor.git
cd "insait-edit-code-editor"
dotnet restore "Insait Edit Code Editor.sln"
dotnet build "Insait Edit Code Editor.sln" -c Release
```

Run the application:

```powershell
dotnet run --project "Insait Edit Code Editor/Insait Edit Code Editor.csproj"
```

## Important: GitHub OAuth File

The public repository intentionally excludes:

```text
Insait Edit Code Editor/Services/GitHubOAuthService.cs
```

This file contains the GitHub OAuth application configuration used by the account and GitHub integration features. It is listed in `.gitignore` and must remain local. Because the rest of the project references its types, the solution requires a local `GitHubOAuthService.cs` implementation to compile.

If you are building from this public repository, add your own `GitHubOAuthService.cs` that matches the service API used by `GitHubAccountService`, `AccountPanelControl`, and `GitWindow`. Do not commit private OAuth credentials.

## Project Structure

```text
Insait Edit Code Editor/
├── Analyzer/                     # Roslyn analyzer project
├── Insait Edit Code Editor/      # Main Avalonia desktop application
│   ├── Controls/                 # Editor, terminal, Git, settings, and account controls
│   ├── Icons/
│   ├── Insait Code Editor/       # Custom editor implementation
│   ├── Interface Localization/   # Built-in AXAML language dictionaries
│   ├── Models/
│   ├── Screenshots/
│   ├── Services/                 # Build, Git, publish, MSIX, settings, and editor services
│   ├── Terminal/
│   ├── ToastNotifications/
│   └── ViewModels/
├── Insait Edit Code Editor.sln
├── LICENSE
└── README.md
```

## Technology Stack

| Component | Technology |
|---|---|
| Target framework | `.NET 10.0` (`net10.0-windows`) |
| UI framework | Avalonia 12 |
| Code analysis | Roslyn 5.x |
| F# support | `FSharp.Compiler.Service` 43.x |
| Build layer | `Microsoft.Build` |
| NuGet integration | `NuGet.Protocol` |
| GitHub integration | Octokit |
| Local storage | LiteDB |

## License

This project is licensed under the **MIT License** — see the [LICENSE](LICENSE) file for details.

> **Note:** The application's UI styles, icons, and visual assets are excluded from the MIT License and remain All Rights Reserved. See [LICENSE](LICENSE) for the full exclusion list.
