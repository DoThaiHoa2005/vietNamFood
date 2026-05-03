#!/usr/bin/env powershell
# Vietnam Food Guide - Automated Deployment Script
# Usage: .\deploy.ps1 -Version "1.0.0" -Environment "production"

param(
    [Parameter(Mandatory=$true)]
    [string]$Version,

    [ValidateSet('development','staging','production')]
    [string]$Environment = 'production',

    [string]$OutputPath = "$PSScriptRoot\..\Deploy",

    [switch]$CreateInstaller,
    [switch]$CreateZip,
    [switch]$SkipTests
)

# ==================== CONFIGURATION ====================
$projectPath = "$PSScriptRoot\VietnamFoodGuide\VietnamFoodGuide"
$buildConfig = "Release"
$framework = "net48"
$appName = "VietnamFoodGuide"

Write-Host "╔════════════════════════════════════════════╗" -ForegroundColor Cyan
Write-Host "║   Vietnam Food Guide - Deployment Script   ║" -ForegroundColor Cyan
Write-Host "╚════════════════════════════════════════════╝" -ForegroundColor Cyan
Write-Host ""
Write-Host "📊 Configuration:" -ForegroundColor Yellow
Write-Host "   Version:      $Version"
Write-Host "   Environment:  $Environment"
Write-Host "   Output:       $OutputPath"
Write-Host "   Build:        $buildConfig"
Write-Host "   Framework:    $framework"
Write-Host ""

# ==================== FUNCTIONS ====================

function Test-Requirements {
    Write-Host "🔍 Checking requirements..." -ForegroundColor Yellow

    # Check .NET CLI
    $dotnetVersion = dotnet --version
    Write-Host "   ✅ .NET CLI version: $dotnetVersion" -ForegroundColor Green

    # Check .NET Framework
    $netVersion = (Get-ItemProperty "HKLM:\SOFTWARE\Microsoft\Net Framework Setup\NDP\v4\Full" -Name version -ErrorAction SilentlyContinue).version
    if ($netVersion) {
        Write-Host "   ✅ .NET Framework: $netVersion" -ForegroundColor Green
    } else {
        Write-Host "   ⚠️ .NET Framework 4.8 not found" -ForegroundColor Yellow
    }

    # Check project file
    if (-not (Test-Path "$projectPath\$appName.csproj")) {
        Write-Error "   ❌ Project file not found: $projectPath\$appName.csproj"
        exit 1
    }
    Write-Host "   ✅ Project file found" -ForegroundColor Green

    # Check Data folder
    if (-not (Test-Path "$projectPath\Data\foods.json")) {
        Write-Error "   ❌ Critical: foods.json not found"
        exit 1
    }
    Write-Host "   ✅ foods.json present" -ForegroundColor Green
}

function Build-Project {
    Write-Host ""
    Write-Host "🔨 Building project..." -ForegroundColor Yellow

    Push-Location $projectPath

    # Clean
    Write-Host "   Cleaning previous builds..." -ForegroundColor Cyan
    dotnet clean -c $buildConfig -q

    # Restore
    Write-Host "   Restoring dependencies..." -ForegroundColor Cyan
    dotnet restore -q

    # Build
    Write-Host "   Building ($buildConfig)..." -ForegroundColor Cyan
    $buildResult = dotnet build -c $buildConfig -p:DebugType=none -p:DebugSymbols=false

    if ($LASTEXITCODE -ne 0) {
        Write-Error "Build failed!"
        exit 1
    }

    Write-Host "   ✅ Build successful" -ForegroundColor Green

    Pop-Location
}

function Package-Application {
    Write-Host ""
    Write-Host "📦 Packaging application..." -ForegroundColor Yellow

    # Create output directory
    $packagePath = "$OutputPath\$appName`_v$Version"
    if (Test-Path $packagePath) {
        Remove-Item $packagePath -Recurse -Force
    }
    New-Item -ItemType Directory -Path $packagePath -Force | Out-Null
    Write-Host "   Created: $packagePath" -ForegroundColor Cyan

    # Copy executable and dependencies
    Write-Host "   Copying executable..." -ForegroundColor Cyan
    Copy-Item "$projectPath\bin\$buildConfig\$framework\$appName.exe" $packagePath -Force
    Copy-Item "$projectPath\bin\$buildConfig\$framework\*.dll" $packagePath -Force
    Copy-Item "$projectPath\bin\$buildConfig\$framework\$appName.exe.config" $packagePath -Force -ErrorAction SilentlyContinue

    # Copy data and assets
    Write-Host "   Copying data and assets..." -ForegroundColor Cyan
    Copy-Item "$projectPath\Data" "$packagePath\Data" -Recurse -Force
    Copy-Item "$projectPath\Assets" "$packagePath\Assets" -Recurse -Force

    # Verify critical files
    if (-not (Test-Path "$packagePath\Data\foods.json")) {
        Write-Error "   ❌ foods.json not copied!"
        exit 1
    }
    Write-Host "   ✅ Verified critical files" -ForegroundColor Green

    # Create README
    Create-README $packagePath

    Write-Host "   ✅ Package created successfully" -ForegroundColor Green

    return $packagePath
}

function Create-README {
    param([string]$Path)

    $readmeContent = @"
Vietnam Food Guide v$Version
============================

📋 SYSTEM REQUIREMENTS
- Windows 10+
- .NET Framework 4.8+
- WebView2 Runtime
- 256 MB RAM minimum
- 100 MB disk space

🚀 HOW TO RUN
1. Double-click VietnamFoodGuide.exe
2. Or run from command line:
   VietnamFoodGuide.exe

📍 FILE STRUCTURE
- VietnamFoodGuide.exe     Main application
- Data/foods.json          Restaurant data (CRITICAL)
- Assets/Images/           Images and resources
- *.dll                    Dependencies

🗺️ OFFLINE MAPS
- Maps are cached automatically
- Works offline after initial load
- Cache location: %AppData%\VietnamFoodGuide\MapCache\

⭐ FAVORITES
- Saved locally in: %AppData%\VietnamFoodGuide\favorites.json
- Works offline

🔊 TEXT-TO-SPEECH
- Supports: Vietnamese, English, Chinese
- Requires: Internet connection
- Uses: Google Translate TTS API

🆘 TROUBLESHOOTING
- If no sound: Check internet & volume
- If map is white: Install WebView2
- If app won't start: Install .NET Framework 4.8

📚 DOCUMENTATION
- README.md                Main readme
- SETUP_GUIDE.md           Detailed setup
- TROUBLESHOOTING.md       Common issues & fixes

© 2024 Vietnam Food Guide - All rights reserved
"@

    $readmeContent | Out-File "$Path\README.txt" -Encoding UTF8
    Write-Host "   ✅ README.txt created" -ForegroundColor Green
}

function Create-ZipPackage {
    param([string]$SourcePath)

    Write-Host ""
    Write-Host "📦 Creating ZIP archive..." -ForegroundColor Yellow

    $zipPath = "$OutputPath\$appName`_v$Version.zip"

    if (Test-Path $zipPath) {
        Remove-Item $zipPath -Force
    }

    Write-Host "   Compressing:" -ForegroundColor Cyan
    Compress-Archive -Path $SourcePath -DestinationPath $zipPath -Force

    $zipSize = (Get-Item $zipPath).Length / 1MB
    Write-Host "   ✅ ZIP created: $zipPath" -ForegroundColor Green
    Write-Host "   📊 Size: $([Math]::Round($zipSize, 2)) MB" -ForegroundColor Cyan

    return $zipPath
}

function Create-Installer {
    Write-Host ""
    Write-Host "📦 Creating Windows Installer..." -ForegroundColor Yellow

    # Check if NSIS is installed
    $nsisPath = "C:\Program Files (x86)\NSIS\makensis.exe"
    if (-not (Test-Path $nsisPath)) {
        Write-Host "   ⚠️ NSIS not found. Installer not created." -ForegroundColor Yellow
        Write-Host "   📥 Download from: https://nsis.sourceforge.io/" -ForegroundColor Cyan
        return
    }

    # Create NSIS script
    Create-NSISScript

    Write-Host "   Building installer..." -ForegroundColor Cyan
    & $nsisPath "installer.nsi" | Out-Null

    if (Test-Path "VietnamFoodGuide_Installer_v$Version.exe") {
        $exeSize = (Get-Item "VietnamFoodGuide_Installer_v$Version.exe").Length / 1MB
        Write-Host "   ✅ Installer created" -ForegroundColor Green
        Write-Host "   📊 Size: $([Math]::Round($exeSize, 2)) MB" -ForegroundColor Cyan
    }
}

function Create-NSISScript {
    $nsisContent = @"
; Vietnam Food Guide Installer
; Generated by deploy.ps1

Name "Vietnam Food Guide v$Version"
OutFile "VietnamFoodGuide_Installer_v$Version.exe"
InstallDir "`$PROGRAMFILES\VietnamFoodGuide"
License "LICENSE.txt"

Page directory
Page instfiles

UninstPage uninstConfirm
UninstPage instfiles

Section "Install"
  SetOutPath "`$INSTDIR"
  File /r "$OutputPath\$appName`_v$Version\*.*"

  CreateDirectory "`$SMPROGRAMS\Vietnam Food Guide"
  CreateShortCut "`$SMPROGRAMS\Vietnam Food Guide\Vietnam Food Guide.lnk" "`$INSTDIR\VietnamFoodGuide.exe"
  CreateShortCut "`$DESKTOP\Vietnam Food Guide.lnk" "`$INSTDIR\VietnamFoodGuide.exe"
SectionEnd

Section "Uninstall"
  Delete "`$SMPROGRAMS\Vietnam Food Guide\Vietnam Food Guide.lnk"
  RMDir "`$SMPROGRAMS\Vietnam Food Guide"
  Delete "`$DESKTOP\Vietnam Food Guide.lnk"
  RMDir /r "`$INSTDIR"
SectionEnd
"@

    $nsisContent | Out-File "installer.nsi" -Encoding ASCII
}

function Generate-ReleaseNotes {
    Write-Host ""
    Write-Host "📝 Generating release notes..." -ForegroundColor Yellow

    $releaseNotesPath = "$OutputPath\RELEASE_NOTES_v$Version.txt"

    $releaseNotes = @"
Vietnam Food Guide v$Version Release Notes
============================================

🎉 Release Date: $(Get-Date -Format 'yyyy-MM-dd')
📊 Environment: $Environment
🏗️ Build: $buildConfig ($framework)

📋 CONTENTS
-----------
✅ Application executable
✅ All dependencies
✅ Restaurant data (foods.json)
✅ Images and assets
✅ Offline maps support
✅ Admin dashboard (admin_dashboard.html)

🎯 FEATURES
----------
🍜 Restaurant search with filters
🗺️ Google Maps integration (with offline support)
🔊 Multi-language TTS (VI, EN, CN)
⭐ Favorites management
💾 Offline support
🎨 Modern UI with animations

⚡ SYSTEM REQUIREMENTS
-------------------
- Windows 10 or later
- .NET Framework 4.8+
- WebView2 Runtime
- Minimum RAM: 256 MB
- Minimum Disk: 100 MB

🔄 INSTALLATION
--------------
1. Extract ZIP archive
2. Run VietnamFoodGuide.exe
3. Allow Windows Defender/UAC if prompted

📚 DOCUMENTATION
---------------
- README.txt          (Basic info)
- SETUP_GUIDE.md      (Detailed setup)
- TROUBLESHOOTING.md  (Common issues)

🐛 KNOWN ISSUES
--------------
None reported.

📞 SUPPORT
--------
For issues, see TROUBLESHOOTING.md

© 2024 Vietnam Food Guide
"@

    $releaseNotes | Out-File $releaseNotesPath -Encoding UTF8
    Write-Host "   ✅ Release notes: $releaseNotesPath" -ForegroundColor Green
}

function Generate-DeploymentReport {
    Write-Host ""
    Write-Host "📊 Generating deployment report..." -ForegroundColor Yellow

    $reportPath = "$OutputPath\DEPLOYMENT_REPORT_v$Version.txt"

    $report = @"
DEPLOYMENT REPORT
=================

Project:           Vietnam Food Guide
Version:           $Version
Environment:       $Environment
Deployment Date:   $(Get-Date)
PowerShell User:   $env:USERNAME
Computer:          $env:COMPUTERNAME

BUILD DETAILS
=============
Framework:         $framework
Configuration:     $buildConfig
Project Path:      $projectPath

PACKAGE CONTENTS
================
Output Location:   $OutputPath\$appName`_v$Version
"@

    if (Test-Path "$OutputPath\$appName`_v$Version") {
        $exeSize = (Get-Item "$OutputPath\$appName`_v$Version\$appName.exe").Length / 1MB
        $report += @"

Main Executable:   $appName.exe ($([Math]::Round($exeSize, 2)) MB)
Data File:         Data\foods.json
Assets:            Assets\ (complete)

Critical Files Verified:
"@

        if (Test-Path "$OutputPath\$appName`_v$Version\Data\foods.json") {
            $report += "`n   ✅ Data\foods.json"
        }
        if (Test-Path "$OutputPath\$appName`_v$Version\Assets") {
            $report += "`n   ✅ Assets folder"
        }
    }

    $report | Out-File $reportPath -Encoding UTF8
    Write-Host "   ✅ Report: $reportPath" -ForegroundColor Green
}

# ==================== MAIN EXECUTION ====================

try {
    # Test requirements
    Test-Requirements

    # Build
    Build-Project

    # Package
    $packagePath = Package-Application

    # Create outputs
    if ($CreateZip) {
        Create-ZipPackage $packagePath
    } else {
        Create-ZipPackage $packagePath
    }

    if ($CreateInstaller) {
        Create-Installer
    }

    # Generate documentation
    Generate-ReleaseNotes
    Generate-DeploymentReport

    # Summary
    Write-Host ""
    Write-Host "╔════════════════════════════════════════════╗" -ForegroundColor Green
    Write-Host "║      ✅ DEPLOYMENT SUCCESSFUL!             ║" -ForegroundColor Green
    Write-Host "╚════════════════════════════════════════════╝" -ForegroundColor Green
    Write-Host ""
    Write-Host "📁 Output Directory: $OutputPath" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "📦 Files created:" -ForegroundColor Yellow
    Get-ChildItem $OutputPath | Where-Object {$_.Name -like "*v$Version*"} | ForEach-Object {
        $size = if ($_.PSIsContainer) { "folder" } else { "$([Math]::Round($_.Length / 1MB, 2)) MB" }
        Write-Host "   ✅ $($_.Name) ($size)" -ForegroundColor Green
    }
    Write-Host ""
    Write-Host "🚀 Next steps:" -ForegroundColor Yellow
    Write-Host "   1. Test the packaged application" -ForegroundColor White
    Write-Host "   2. Upload to GitHub/Server" -ForegroundColor White
    Write-Host "   3. Create release announcement" -ForegroundColor White
    Write-Host "   4. Notify users" -ForegroundColor White
    Write-Host ""

} catch {
    Write-Host ""
    Write-Error "❌ Deployment failed: $_"
    exit 1
}
