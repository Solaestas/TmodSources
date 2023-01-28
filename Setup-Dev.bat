@echo off
cd %~dp0%
dotnet run --project ModTools/Setup/Setup.csproj -p:DefineConstants=DEV
dotnet publish ModTools/ModTools/ModTools.csproj -p:PublishProfile=FolderProfile -p:DefineConstants=DEV
dotnet publish ModTools/ContentBuilder/ContentBuilder.csproj -p:PublishProfile=FolderProfile
