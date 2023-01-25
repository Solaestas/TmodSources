# Setup

运行Setup.bat会自动搜索tModLoader路径并生成Config.props与Config.cs文件，同时进行Tasks的编译

Setup结束后即可将模组源码放入ModSources文件夹中进行编译

如果使用的是Dev版tML，则是运行Setup-Dev.bat

# Usage

-   自动编译fx文件为xnb，并且存入bin文件夹中，打包时路径为fx文件路径
-   预处理png文件为rawimg，初次编译会较慢，但是后续会加快
-   可能比tModLoader编译速度更快
-   Mod名自动设置为程序集名称（程序集名称和Mod名称必须一致），而不是文件夹名称
-   自动打包依赖dll（包括NuGet）
-   禁用架构警告

## Resource

默认情况根据build.txt打包资源，可以设置<IgnoreBuildFile>属性为true来启用自定义打包

>   自定义打包中不会读取全部文件然后无视部分文件，而是会打包所有ItemGroup - ResourceFile所包含的文件
>
>   如果需要向ResourceFile添加文件，可以向csproj添加<ResourceFile Include="" />

## Enable Mod

每次生成Mod会调整tML的Mod启用列表，默认情况会向其中加入正在编译的Mod

可以设置HelpMods属性来增加自动启用的Mod，如我想启用Hero，可以使用<HelpMods>HEROsMod<HelpMods>

可以通过设置DisableOtherMod为true属性来自动禁用其他Mod，示例可以参考UwUMod

## AssetPath Generator

自动生成路径引用的代码，通过添加<EnablePathGenerator>启用

## Publicizer

使用Nuget包BepInEx.AssemblyPublicizer.MSBuild，默认添加tModLoader，通过添加<EnablePublicizer>启用

  # TODO

Hook掉UIModSourcesItem，支持发布Mod