<#
.SYNOPSIS
    Migrates a .NET solution from its current version to a target .NET version.

.DESCRIPTION
    Run this script from the solution root (where the .sln lives).
    It detects the current target framework(s), asks which version to
    migrate to, updates all .csproj TFMs and global.json, cleans build
    artifacts, updates NuGet packages and rebuilds the solution.

.NOTES
    Tested on: Windows + PowerShell 5.1 / PowerShell 7+
    Requires : .NET 10 SDK installed (dotnet --list-sdks)
#>

[CmdletBinding()]
param(
    # Optional: pass -TargetVersion 10 to skip the interactive prompt.
    [int]$TargetVersion,

    # Skip the "dotnet list package --outdated" / package bump step.
    [switch]$SkipPackages,

    # Automatically bump outdated packages to their real latest version
    # (read from "dotnet list package --outdated"), covering ALL packages.
    [switch]$AutoUpdatePackages,

    # With -AutoUpdatePackages: only bump Microsoft framework packages
    # (Microsoft.AspNetCore.*, Microsoft.EntityFrameworkCore.*, etc.).
    [switch]$FrameworkPackagesOnly,

    # With -AutoUpdatePackages: also consider prerelease versions as "latest".
    [switch]$IncludePrerelease,

    # With -AutoUpdatePackages: package IDs to never bump (case-insensitive).
    # Accepts exact names or wildcards, e.g. -ExcludePackages 'FluentAssertions','Serilog.*'.
    [string[]]$ExcludePackages = @(),

    # Only show what would change, without writing files or building.
    [switch]$WhatIfMode
)

$ErrorActionPreference = 'Stop'
$root = Get-Location

function Write-Step { param($msg) Write-Host "`n=== $msg ===" -ForegroundColor Cyan }
function Write-Ok   { param($msg) Write-Host "  [OK] $msg"   -ForegroundColor Green }
function Write-Warn { param($msg) Write-Host "  [!]  $msg"   -ForegroundColor Yellow }

# Package name prefixes that follow the .NET major version (used only with
# -FrameworkPackagesOnly).
$FrameworkPackagePrefixes = @(
    'Microsoft.AspNetCore.',
    'Microsoft.EntityFrameworkCore.',
    'Microsoft.Extensions.',
    'System.Net.Http.Json'
)

# ---------------------------------------------------------------------------
# 0. Sanity checks
# ---------------------------------------------------------------------------
Write-Step "Checking environment"

if (-not (Get-Command dotnet -ErrorAction SilentlyContinue)) {
    throw "The 'dotnet' CLI was not found in PATH."
}

$sln = Get-ChildItem -Path $root -Filter *.sln -File | Select-Object -First 1
if ($null -eq $sln) {
    $slnx = Get-ChildItem -Path $root -Filter *.slnx -File | Select-Object -First 1
    if ($slnx) { $sln = $slnx }
}
if ($sln) { Write-Ok "Solution found: $($sln.Name)" }
else      { Write-Warn "No .sln/.slnx found in $root - continuing with project files only." }

$installedSdks = (dotnet --list-sdks) -join "`n"
Write-Host "Installed SDKs:`n$installedSdks" -ForegroundColor DarkGray

# ---------------------------------------------------------------------------
# 1. Discover all project files and detect current version(s)
# ---------------------------------------------------------------------------
Write-Step "Detecting current target framework(s)"

$projects = Get-ChildItem -Path $root -Recurse -Include *.csproj, *.vbproj, *.fsproj -File `
            | Where-Object { $_.FullName -notmatch '\\(bin|obj)\\' }

if ($projects.Count -eq 0) { throw "No project files (.csproj/.vbproj/.fsproj) found under $root." }

$tfmRegex   = '<TargetFramework>\s*net(\d+)\.0\s*</TargetFramework>'
$tfmsRegex  = '<TargetFrameworks>\s*([^<]+?)\s*</TargetFrameworks>'
$detected   = @{}   # major version number -> count

foreach ($proj in $projects) {
    $content = Get-Content $proj.FullName -Raw

    if ($content -match $tfmRegex) {
        $ver = [int]$Matches[1]
        $detected[$ver] = ($detected[$ver] + 1)
        Write-Host ("  {0,-45} net{1}.0" -f $proj.Name, $ver)
    }
    elseif ($content -match $tfmsRegex) {
        Write-Host ("  {0,-45} (multi) {1}" -f $proj.Name, $Matches[1]) -ForegroundColor DarkGray
        foreach ($m in [regex]::Matches($content, 'net(\d+)\.0')) {
            $ver = [int]$m.Groups[1].Value
            $detected[$ver] = ($detected[$ver] + 1)
        }
    }
    else {
        Write-Warn "$($proj.Name): no net#.0 TFM detected (maybe SDK-style multi-target or non-.NET)."
    }
}

if ($detected.Count -eq 0) { throw "Could not detect any current .NET version." }

$currentMajor = ($detected.Keys | Measure-Object -Maximum).Maximum
Write-Ok ("Current version detected: net{0}.0" -f $currentMajor)
if ($detected.Count -gt 1) {
    Write-Warn ("Mixed versions detected: {0}" -f (($detected.Keys | Sort-Object | ForEach-Object { "net$_.0" }) -join ', '))
}

# ---------------------------------------------------------------------------
# 2. Ask which version to migrate to
# ---------------------------------------------------------------------------
Write-Step "Target version"

if (-not $TargetVersion) {
    $suggested = $currentMajor + 1
    $answer = Read-Host "Which .NET version do you want to migrate to? (major number, e.g. $suggested)"
    if ([string]::IsNullOrWhiteSpace($answer)) { $TargetVersion = $suggested }
    else                                        { $TargetVersion = [int]$answer }
}

if ($TargetVersion -lt $currentMajor) {
    Write-Warn "Target net$TargetVersion.0 is older than current net$currentMajor.0."
    $confirm = Read-Host "Continue anyway? (y/N)"
    if ($confirm -ne 'y') { Write-Host "Aborted."; return }
}
elseif ($TargetVersion -eq $currentMajor) {
    Write-Warn "Already on net$currentMajor.0 - no TFM change needed."
    if (-not $AutoUpdatePackages) {
        $confirm = Read-Host "Continue to only refresh packages/build? (y/N)"
        if ($confirm -ne 'y') { Write-Host "Aborted."; return }
    } else {
        Write-Ok "Continuing to refresh packages to their latest versions."
    }
}

$oldTfm = "net$currentMajor.0"
$newTfm = "net$TargetVersion.0"
Write-Ok "Target: $oldTfm  ->  $newTfm"

# ---------------------------------------------------------------------------
# 3. Update TargetFramework(s) in every project
# ---------------------------------------------------------------------------
Write-Step "Updating project files"

$changedCount = 0
foreach ($proj in $projects) {
    $content = Get-Content $proj.FullName -Raw
    $updated = $content

    # Single-target: replace the detected current major only.
    $updated = $updated -replace "<TargetFramework>\s*net$currentMajor\.0\s*</TargetFramework>", "<TargetFramework>$newTfm</TargetFramework>"

    # Multi-target: bump any netX.0 occurrence inside <TargetFrameworks>.
    $updated = [regex]::Replace($updated, "net$currentMajor\.0(-\w+)?", { param($m) "$newTfm$($m.Groups[1].Value)" })

    if ($updated -ne $content) {
        if ($WhatIfMode) {
            Write-Host "  [WhatIf] would update $($proj.Name)" -ForegroundColor Magenta
        } else {
            Set-Content -Path $proj.FullName -Value $updated -NoNewline -Encoding UTF8
            Write-Ok "Updated $($proj.Name)"
        }
        $changedCount++
    }
}
Write-Host "  $changedCount project file(s) affected."

# ---------------------------------------------------------------------------
# 4. Update global.json (if present)
# ---------------------------------------------------------------------------
$globalJson = Get-ChildItem -Path $root -Filter global.json -File -Recurse `
              | Where-Object { $_.FullName -notmatch '\\(bin|obj)\\' } | Select-Object -First 1
if ($globalJson) {
    Write-Step "Updating global.json"
    $json = Get-Content $globalJson.FullName -Raw
    $newSdk = "$TargetVersion.0.100"
    $json2 = $json -replace '("version"\s*:\s*")\d+\.\d+\.\d+(")', "`${1}$newSdk`${2}"
    if ($WhatIfMode) {
        Write-Host "  [WhatIf] would set SDK version to $newSdk" -ForegroundColor Magenta
    } elseif ($json2 -ne $json) {
        Set-Content -Path $globalJson.FullName -Value $json2 -NoNewline -Encoding UTF8
        Write-Ok "global.json SDK version -> $newSdk"
    } else {
        Write-Warn "global.json found but SDK version not updated (check format)."
    }
}

if ($WhatIfMode) { Write-Host "`nWhatIf mode: no build/package changes performed." -ForegroundColor Magenta; return }

# ---------------------------------------------------------------------------
# 5. Clean bin/obj
# ---------------------------------------------------------------------------
Write-Step "Cleaning build artifacts"
if ($sln) { dotnet clean $sln.FullName | Out-Null }
Get-ChildItem -Path $root -Recurse -Directory -Include bin, obj `
    | Where-Object { $_.FullName -notmatch '\\node_modules\\' } `
    | ForEach-Object { Remove-Item $_.FullName -Recurse -Force -ErrorAction SilentlyContinue }
Write-Ok "bin/obj folders removed"

# ---------------------------------------------------------------------------
# 6. Restore + report / auto-update packages to their REAL latest version
# ---------------------------------------------------------------------------
if (-not $SkipPackages) {
    Write-Step "Restoring and checking outdated NuGet packages"
    dotnet restore | Out-Null

    if ($AutoUpdatePackages) {
        # --- Build a map: packageId(lower) -> latest available version -------
        $listArgs = @('list', 'package', '--outdated', '--format', 'json')
        if ($IncludePrerelease) { $listArgs += '--include-prerelease' }

        $jsonRaw   = (& dotnet @listArgs) 2>$null | Out-String
        $latestMap = @{}

        try {
            $data = $jsonRaw | ConvertFrom-Json
            foreach ($p in $data.projects) {
                if (-not $p.frameworks) { continue }
                foreach ($fw in $p.frameworks) {
                    foreach ($pkg in $fw.topLevelPackages) {
                        if ([string]::IsNullOrWhiteSpace($pkg.latestVersion)) { continue }
                        $key = $pkg.id.ToLowerInvariant()
                        # Keep the highest latestVersion if a package appears under several TFMs.
                        if (-not $latestMap.ContainsKey($key) -or
                            ([version]($pkg.latestVersion -replace '-.*$','') -gt [version]($latestMap[$key] -replace '-.*$',''))) {
                            $latestMap[$key] = $pkg.latestVersion
                        }
                    }
                }
            }
        }
        catch {
            Write-Warn "Could not parse 'dotnet list package --outdated --format json'."
            Write-Warn "Your SDK might not support --format json. Skipping auto-update."
        }

        if ($latestMap.Count -eq 0) {
            Write-Warn "No outdated packages found on the configured feeds."
        }
        else {
            $scope = if ($FrameworkPackagesOnly) { "Microsoft framework packages" } else { "ALL outdated packages" }
            Write-Step "Auto-updating $scope to their latest versions"

            $bumped = 0
            $pkgRefRegex = '<PackageReference\s+Include="([^"]+)"\s+Version="([^"]+)"\s*/?>'

            foreach ($proj in $projects) {
                $content = Get-Content $proj.FullName -Raw
                $updated = $content

                foreach ($match in [regex]::Matches($content, $pkgRefRegex)) {
                    $pkgName    = $match.Groups[1].Value
                    $pkgVersion = $match.Groups[2].Value
                    $key        = $pkgName.ToLowerInvariant()

                    if (-not $latestMap.ContainsKey($key)) { continue }   # not outdated
                    $latest = $latestMap[$key]
                    if ($pkgVersion -eq $latest) { continue }

                    # Skip explicitly excluded packages (exact match or wildcard).
                    $isExcluded = $false
                    foreach ($pattern in $ExcludePackages) {
                        if ($pkgName -like $pattern) { $isExcluded = $true; break }
                    }
                    if ($isExcluded) {
                        Write-Warn "$($proj.Name): $pkgName skipped (excluded), kept at $pkgVersion"
                        continue
                    }

                    if ($FrameworkPackagesOnly) {
                        $isFramework = $false
                        foreach ($prefix in $FrameworkPackagePrefixes) {
                            if ($pkgName -like "$prefix*") { $isFramework = $true; break }
                        }
                        if (-not $isFramework) { continue }
                    }

                    $original    = $match.Value
                    $replacement = $original -replace 'Version="[^"]+"', "Version=`"$latest`""
                    $updated     = $updated.Replace($original, $replacement)
                    Write-Ok "$($proj.Name): $pkgName $pkgVersion -> $latest"
                    $bumped++
                }

                if ($updated -ne $content) {
                    Set-Content -Path $proj.FullName -Value $updated -NoNewline -Encoding UTF8
                }
            }

            if ($bumped -eq 0) {
                Write-Warn "Nothing to bump (references already at latest, or not matched)."
            } else {
                Write-Host "  $bumped package reference(s) bumped. Restoring..." -ForegroundColor DarkGray
                dotnet restore | Out-Null
                if ($FrameworkPackagesOnly) {
                    Write-Warn "Only Microsoft framework packages were bumped. Re-run without -FrameworkPackagesOnly to update third-party libs too."
                }
            }
        }
    }
    else {
        Write-Host "Outdated packages (review before bumping):" -ForegroundColor DarkGray
        dotnet list package --outdated
        Write-Warn "Tip: re-run with -AutoUpdatePackages to bump everything to the latest versions automatically."
        Write-Warn "     Add -FrameworkPackagesOnly to limit the bump to Microsoft.* packages."
    }
}

# ---------------------------------------------------------------------------
# 7. Build
# ---------------------------------------------------------------------------
Write-Step "Building solution"
$buildTarget = if ($sln) { $sln.FullName } else { $root }
dotnet build $buildTarget --no-incremental
if ($LASTEXITCODE -eq 0) {
    Write-Ok "Build succeeded on $newTfm"
} else {
    Write-Warn "Build finished with errors - review the messages above (breaking changes, package versions)."
}

Write-Step "Done"
Write-Host "Migration $oldTfm -> $newTfm complete. Review breaking changes:" -ForegroundColor Cyan
Write-Host "  https://learn.microsoft.com/dotnet/core/compatibility/$TargetVersion" -ForegroundColor Cyan
