#!/bin/bash
dotnet run --project ModTools/Setup/Setup.csproj
dotnet publish ModTools/ModTools/ModTools.csproj -p:PublishProfile=FolderProfile
dotnet publish ModTools/ContentBuilder/ContentBuilder.csproj -p:PublishProfile=FolderProfile
dotnet build ModTools/UwUMod/UwUMod.csproj