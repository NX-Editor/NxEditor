# Nintendo Extended Editor (NX-Editor)

[![License](https://img.shields.io/badge/License-AGPL%20v3.0-blue.svg)](License.txt) [![Downloads](https://img.shields.io/github/downloads/NX-Editor/NxEditor/total)](https://github.com/NX-Editor/NxEditor/releases)

A general editor for editing first-party Nintendo formats. Primarily aimed at support for Tears of the Kingdom files and more modern Nintendo Switch files.

## Setup

1. Install the [.NET 8 runtime](https://dotnet.microsoft.com/en-us/download/dotnet/8.0) ([Windows x64 direct download]([https://dotnet.microsoft.com/en-us/download/dotnet/8.0](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)))
2. Download the [latest release](https://github.com/NX-Editor/NxEditor/releases/latest) (Launcher), extract the zip, and run the exe
3. Toggle the plugins you would like to install
4. Click **Install NX Editor** and wait for the installation to complete

## Building from Source (Windows)

### Requirments

- [git](https://git-scm.com/)
- [DotNet SDK 7.0+](https://dotnet.microsoft.com/en-us/download)

### Instructions

1. Clone the [GitHub repository](https://github.com/NX-Editor/NxEditor)
   
   ```powershell
   git clone --recursive "https://github.com/NX-Editor/NxEditor"; cd "NxEditor"
   ```
2. Build the project
   
   ```powershell
   cd "./src/NxEditor/"; dotnet build
   ```
3. Run the project
   
   ```powershell
   dotnet run
   ```

---

<details><summary>PowerShell Script & Command</summary>
<p>

```powershell
git clone --recursive "https://github.com/NX-Editor/NxEditor"
cd "NxEditor"
cd "./src/NxEditor/"
dotnet build
dotnet run
```

```powershell
git clone --recursive "https://github.com/NX-Editor/NxEditor"; cd "NxEditor"; cd "./src/NxEditor/"; dotnet build; dotnet run
```

</p>
</details> 
