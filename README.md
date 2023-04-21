# TmodSource

This is an implement of packing tmod file via MSBuild Task with some common tools

## Installation

1.   Download the binary file in accord with your tModLoader version,
2.   Run **Setup.bat** (or **Setup.sh**), it will find the location of tModloader and compile the tasks, you need to input the location of tModLoader manually if it can't find tModLoader.
3.   Move your mod source folder into the **ModSources** directory in this repository
4.   Build your mod as usual

```sh
git clone -b <branch in accord with your tModLoader version> https://gitee.com/Ye_you_gitee/tmod-source TmodSource
cd TmodSource
./Setup.bat
```

## Supported Version

| Version       | Branch Name |
| ------------- | ----------- |
| Dev           | dev         |
| stable        | stable      |
| 1.4.4 preview | 144preview  |

## Feature

-   Cache the rawimg file

-   Build **\.fx** into **.xnb** and pack them automatically

-   Separated props and targets file allow you to create lib without absolute path

-   Fast building?(not sure, maybe a little slower)

-   **PathGenerator**

    you can add `<EnablePathGenerator>true</EnablePathGenerator>` inside the `<ItemGroup>` in your csproj file to enable **PathGenerator**

    It can generate path reference to textures and effects
-	 **Publicizer**

    add `<EnablePublicizer>true</EnablePublicizer>` inside the `<ItemGroup>` in your csproj file to use 	**BepInEx.AssemblyPublicizer.MSBuild**, *tModLoader.dll and MonoMod.RuntimeDetour.dll is publicized as default*



example
    

```xml
<?xml version="1.0" encoding="utf-8"?>
<Project Sdk="Microsoft.NET.Sdk">
	<Import Project="..\tModLoader.targets" />
	<PropertyGroup>
		<EnablePathGenerator>true</EnablePathGenerator>
		<EnablePublicizer>true</EnablePublicizer>
	</PropertyGroup>
	<ItemGroup>
		<PackageReference Include="tModLoader.CodeAssist" Version="0.1.*" />
	</ItemGroup>
</Project>

```


​    
