function Start-Iterate($path) {
    foreach ($item in Get-ChildItem -Path $path) {
        Invoke-Build-Native "$path$item"
    }
}

function Invoke-Build-Native($path) {
    if (@(Test-Path "$path\lib\") -eq $True) {
        Iterate "$path\lib\"
    }

    if (@(Test-Path ($path = "$path\native")) -eq $False) {
        Return
    }

    Update-CMake $path
    Invoke-Build-CMake $path
}

function Update-CMake($path) {
    Try {
        New-Item "$path/build" -ItemType Directory -Force | Out-Null
        cmake --no-warn-unused-cli -DCMAKE_EXPORT_COMPILE_COMMANDS:BOOL=TRUE -DCMAKE_BUILD_TYPE:STRING=Release -S "$path" -B "$path\build" -G "Ninja"
    }
    Catch {
        Write-Output "'$path' - Configure failed"
    }
}

function Invoke-Build-CMake($path) {
    Try {
        cmake --build "$path/build" --config Release --target all -j 4
    }
    Catch {
        Write-Output "'$path' - Build failed"
    }
}

Start-Iterate ".\lib\"
Read-Host -Prompt "`n`nPress any key to continue"