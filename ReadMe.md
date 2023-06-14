# Nintendo Extended Editor (NX-Editor)

[![License](https://img.shields.io/badge/License-AGPL%20v3.0-blue.svg)](License.txt) [![Downloads](https://img.shields.io/github/downloads/NX-Editor/NxEditor/total)](https://github.com/NX-Editor/NxEditor/releases)

A general editor for editing first-party Nintendo formats. Primary aimed at support for Tears of the Kingdom files and more modern Nintendo Switch files.

## Setup

1. Install the [.NET 7 runtime](https://dotnet.microsoft.com/en-us/download/dotnet/7.0) ([Windows x64 direct download](https://dotnet.microsoft.com/en-us/download/dotnet/thank-you/runtime-7.0.5-windows-x64-installer))
2. Download the [latest release](https://github.com/NX-Editor/NxEditor/releases/latest) and run the exe
3. Fill out the required settings (more info in the app)

## Building from Source (Windows)

### Requirments

- [git](https://git-scm.com/)
- [DotNet SDK 7.0+](https://dotnet.microsoft.com/en-us/download)
- [CMake](https://cmake.org/)
- [Ninja](https://github.com/ninja-build/ninja/releases)
- [GCC/MinGW](https://github.com/brechtsanders/winlibs_mingw/releases/)

### Instructions

1. Clone the [GitHub repository](https://github.com/NX-Editor/NxEditor)
   
   ```powershell
   git clone "https://github.com/NX-Editor/NxEditor"; cd "NxEditor"
   ```
2. Update the submodules
   
   ```powershell
   git submodule update --init --recursive
   ```
3. Run the build script (`build.ps1`)
   
   ```powershell
   .\build.ps1
   ```
4. Build the project
   
   ```powershell
   dotnet build "./src/NxEditor/NxEditor.csproj"
   ```
4. Run the project
   
   ```powershell
   cd "./src/NxEditor/"; dotnet run
   ```

---

<details><summary>PowerShell Script & Command</summary>
<p>

```powershell
git clone "https://github.com/NX-Editor/NxEditor"
cd "NxEditor"
git submodule update --init --recursive
".\build.ps1"
cd "./src/NxEditor/"
dotnet build
dotnet run
```

```powershell
git clone "https://github.com/NX-Editor/NxEditor"; cd "NxEditor"; git submodule update --init --recursive; ".\build.ps1"; cd "./src/NxEditor/"; dotnet build; dotnet run
```

</p>
</details> 