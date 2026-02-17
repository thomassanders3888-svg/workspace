# TerraForge Build Script
# PowerShell 7+ required

param(
    [Parameter()]
    [ValidateSet("all", "client", "server", "steam", "web")]
    [string]$Target = "all",
    
    [Parameter()]
    [ValidateSet("Debug", "Release")]
    [string]$Configuration = "Release",
    
    [Parameter()]
    [string]$Version = "0.1.0-alpha",
    
    [Parameter()]
    [switch]$UploadToSteam
)

$ErrorActionPreference = "Stop"
$BuildRoot = "$PSScriptRoot/../build"
$VersionTag = $Version.Replace(".", "_")

Write-Host "TerraForge Build Script v1.0" -ForegroundColor Cyan
Write-Host "Target: $Target | Configuration: $Configuration | Version: $Version" -ForegroundColor Cyan
Write-Host ""

# Create build directories
function Initialize-BuildDirs {
    New-Item -ItemType Directory -Force -Path "$BuildRoot/client/windows" | Out-Null
    New-Item -ItemType Directory -Force -Path "$BuildRoot/client/macos" | Out-Null
    New-Item -ItemType Directory -Force -Path "$BuildRoot/client/linux" | Out-Null
    New-Item -ItemType Directory -Force -Path "$BuildRoot/server" | Out-Null
    New-Item -ItemType Directory -Force -Path "$BuildRoot/webgl" | Out-Null
}

# Build Unity Client for Windows
function Build-Client-Windows {
    Write-Host "Building Unity Client (Windows)..." -ForegroundColor Yellow
    
    $UnityPath = "C:\Program Files\Unity\Hub\Editor\2023.2.0f1\Editor\Unity.exe"
    $ProjectPath = "$PSScriptRoot/../client/TerraForge"
    $OutputPath = "$BuildRoot/client/windows"
    
    if (-not (Test-Path $UnityPath)) {
        Write-Error "Unity not found at $UnityPath"
        return
    }
    
    $BuildArgs = @(
        "-quit",
        "-batchmode",
        "-nographics",
        "-projectPath", $ProjectPath,
        "-executeMethod", "BuildScript.BuildWindows",
        "-buildVersion", $Version
    )
    
    & $UnityPath @BuildArgs
    
    if ($LASTEXITCODE -ne 0) {
        Write-Error "Windows build failed"
        return
    }
    
    Write-Host "✅ Windows build complete: $OutputPath" -ForegroundColor Green
}

# Build Unity Client for Mac
function Build-Client-Mac {
    Write-Host "Building Unity Client (macOS)..." -ForegroundColor Yellow
    # Mac builds typically done on macOS, or use cross-compile
    Write-Host "⚠️  Mac build requires macOS or cross-compilation setup" -ForegroundColor Yellow
}

# Build Unity Client for Linux
function Build-Client-Linux {
    Write-Host "Building Unity Client (Linux)..." -ForegroundColor Yellow
    # Linux builds via Unity Linux build support
    Write-Host "⚠️  Ensure Unity Linux Build Support installed" -ForegroundColor Yellow
}

# Build Unity WebGL
function Build-Client-WebGL {
    Write-Host "Building Unity WebGL..." -ForegroundColor Yellow
    
    $UnityPath = "C:\Program Files\Unity\Hub\Editor\2023.2.0f1\Editor\Unity.exe"
    $ProjectPath = "$PSScriptRoot/../client/TerraForge"
    $OutputPath = "$BuildRoot/webgl"
    
    $BuildArgs = @(
        "-quit", "-batchmode", "-nographics",
        "-projectPath", $ProjectPath,
        "-executeMethod", "BuildScript.BuildWebGL",
        "-buildVersion", $Version
    )
    
    & $UnityPath @BuildArgs
    
    Write-Host "✅ WebGL build complete: $OutputPath" -ForegroundColor Green
}

# Build .NET Server
function Build-Server {
    Write-Host "Building Server (.NET 8)..." -ForegroundColor Yellow
    
    $ServerPath = "$PSScriptRoot/../server/TerraForgeServer"
    $OutputPath = "$BuildRoot/server"
    
    Push-Location $ServerPath
    
    try {
        # Restore dependencies
        dotnet restore
        
        # Publish
        dotnet publish -c $Configuration -r win-x64 --self-contained true -o "$OutputPath/windows"
        dotnet publish -c $Configuration -r linux-x64 --self-contained true -o "$OutputPath/linux"
        
        Write-Host "✅ Server build complete" -ForegroundColor Green
    }
    finally {
        Pop-Location
    }
}

# Upload to Steam (requires SteamCMD)
function Publish-Steam {
    Write-Host "Uploading to Steam..." -ForegroundColor Yellow
    
    $SteamCmd = "C:\steamcmd\steamcmd.exe"
    $AppId = "480" # Replace with your actual App ID
    
    if (-not (Test-Path $SteamCmd)) {
        Write-Error "SteamCMD not found. Install from: https://developer.valvesoftware.com/wiki/SteamCMD"
        return
    }
    
    & $SteamCmd +login anonymous +app_build $PSScriptRoot/app_build.vdf +quit
    
    Write-Host "✅ Steam upload complete" -ForegroundColor Green
}

# Create distribution packages
function New-Distribution {
    Write-Host "Creating distribution packages..." -ForegroundColor Yellow
    
    $ZipWindows = "$BuildRoot/TerraForge-Windows-$Version.zip"
    $ZipLinux = "$BuildRoot/TerraForge-Linux-$Version.zip"
    
    Compress-Archive -Path "$BuildRoot/client/windows/*" -DestinationPath $ZipWindows -Force
    Compress-Archive -Path "$BuildRoot/server/linux/*" -DestinationPath $ZipLinux -Force
    
    Write-Host "✅ Distribution packages created" -ForegroundColor Green
}

# Main execution
switch ($Target) {
    "all" {
        Initialize-BuildDirs
        Build-Client-Windows
        Build-Client-WebGL
        Build-Server
        New-Distribution
        if ($UploadToSteam) { Publish-Steam }
    }
    "client" {
        Initialize-BuildDirs
        Build-Client-Windows
    }
    "server" {
        Initialize-BuildDirs
        Build-Server
    }
    "steam" {
        Build-Client-Windows
        Publish-Steam
    }
    "web" {
        Build-Client-WebGL
    }
}

Write-Host ""
Write-Host "Build complete! Outputs in: $BuildRoot" -ForegroundColor Green
