@echo off
cd /D %~dp0%
for /f "delims=" %%i in ('Tools\FindtModLoader\FindtModLoader.exe') do set tMLPath=%%i
echo ^<Project^> > Config.targets
echo     ^<PropertyGroup^> >> Config.targets
echo         ^<tMLPath^>%tMLPath%^</tMLPath^> >> Config.targets
echo     ^</PropertyGroup^> >> Config.targets
echo ^</Project^> >> Config.targets