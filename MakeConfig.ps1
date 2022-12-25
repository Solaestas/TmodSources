$UninstallPaths = @(,
    'HKLM:\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall',
    'HKCU:\Software\Microsoft\Windows\CurrentVersion\Uninstall'
)

if ([System.Environment]::Is64BitOperatingSystem) {
    $UninstallPaths += 'HKLM:SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall'
}
$Steams = $UninstallPaths | ForEach-Object {
    Get-ChildItem $_ | Where-Object {
        $_.GetValue("DisplayName") -eq "Steam"
    } 
}

if ($Steams.Length -gt 1) {
    Write-Output "?????Multi-Steam??????"
}

$Steam = $Steams[0]

$tMLPath = [System.IO.Path]::Combine( [System.IO.Path]::GetDirectoryName( $Steam.GetValue("DisplayIcon")), 
    "steamapps",
    "common", 
    "tModLoader"
)

"<Project>
    <PropertyGroup>
        <tMLPath>$tMLPath</tMLPath>
    </PropertyGroup>
</Project>" | Out-File "$PSScriptRoot\Config.targets"