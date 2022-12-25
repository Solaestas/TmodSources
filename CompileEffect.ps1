$Path = Get-Location
Set-Location "$PSScriptRoot/EffectCompiler"

.\XNBCompiler.exe $PSScriptRoot

Set-Location $Path
