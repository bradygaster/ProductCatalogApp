# Fix for Roslyn compiler tools missing in bin directory
# This script restores NuGet packages and copies Roslyn files to the bin directory

$ErrorActionPreference = "Stop"

Write-Host "Fixing Roslyn compiler tools issue..." -ForegroundColor Cyan

# Get the script directory
$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$projectDir = Join-Path $scriptDir "ProductCatalog"
$packagesDir = Join-Path $scriptDir "packages"
$roslynSourceDir = Join-Path $packagesDir "Microsoft.CodeDom.Providers.DotNetCompilerPlatform.2.0.1\tools\RoslynLatest"
$binDir = Join-Path $projectDir "bin"
$roslynDestDir = Join-Path $binDir "roslyn"

# Step 1: Restore NuGet packages if needed
if (-not (Test-Path $roslynSourceDir)) {
    Write-Host "Restoring NuGet packages..." -ForegroundColor Yellow
    $slnPath = Join-Path $scriptDir "ProductCatalogApp.sln"
    
    # Try to find nuget.exe or use dotnet restore
    $nugetExe = Get-Command nuget.exe -ErrorAction SilentlyContinue
    
    if ($nugetExe) {
        & nuget.exe restore $slnPath
    } else {
        Write-Host "NuGet.exe not found. Trying MSBuild restore..." -ForegroundColor Yellow
        $msbuild = "C:\Program Files\Microsoft Visual Studio\2022\Enterprise\MSBuild\Current\Bin\MSBuild.exe"
        if (-not (Test-Path $msbuild)) {
            $msbuild = "C:\Program Files\Microsoft Visual Studio\2022\Professional\MSBuild\Current\Bin\MSBuild.exe"
        }
        if (-not (Test-Path $msbuild)) {
            $msbuild = "C:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe"
        }
        if (Test-Path $msbuild) {
            & $msbuild /t:Restore $slnPath
        } else {
            Write-Host "ERROR: Could not find MSBuild or NuGet.exe to restore packages." -ForegroundColor Red
            Write-Host "Please restore NuGet packages manually in Visual Studio (right-click solution > Restore NuGet Packages)" -ForegroundColor Yellow
            exit 1
        }
    }
}

# Step 2: Check if Roslyn files exist in packages
if (-not (Test-Path $roslynSourceDir)) {
    Write-Host "ERROR: Roslyn files not found in packages directory: $roslynSourceDir" -ForegroundColor Red
    Write-Host "Please restore NuGet packages in Visual Studio first." -ForegroundColor Yellow
    exit 1
}

# Step 3: Create bin directory if it doesn't exist
if (-not (Test-Path $binDir)) {
    New-Item -ItemType Directory -Path $binDir -Force | Out-Null
    Write-Host "Created bin directory" -ForegroundColor Green
}

# Step 4: Create roslyn directory in bin
if (-not (Test-Path $roslynDestDir)) {
    New-Item -ItemType Directory -Path $roslynDestDir -Force | Out-Null
    Write-Host "Created roslyn directory in bin" -ForegroundColor Green
}

# Step 5: Copy Roslyn files
Write-Host "Copying Roslyn compiler tools to bin\roslyn..." -ForegroundColor Yellow
Copy-Item -Path "$roslynSourceDir\*" -Destination $roslynDestDir -Recurse -Force

# Verify the copy
if (Test-Path (Join-Path $roslynDestDir "csc.exe")) {
    Write-Host "SUCCESS: Roslyn compiler tools copied successfully!" -ForegroundColor Green
    Write-Host "The csc.exe file is now at: $(Join-Path $roslynDestDir 'csc.exe')" -ForegroundColor Green
    Write-Host "`nYou should now be able to run your web application." -ForegroundColor Cyan
} else {
    Write-Host "WARNING: csc.exe not found after copy. Please check the source directory." -ForegroundColor Red
}
