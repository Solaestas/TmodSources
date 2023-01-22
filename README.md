# Setup

运行Setup.bat会自动搜索tModLoader路径并生成Config.props与Config.cs文件，同时进行Tasks的编译

Setup结束后即可将模组源码放入ModSources文件夹中进行编译

# Usage

-   自动编译fx文件为xnb，并且存入bin文件夹中，打包时路径为fx文件路径
-   预处理png文件为rawimg，加快运行速度
-   可能比tModLoader编译速度更快
-   可修改Mod名，不必与文件夹名称一致
-   自动打包依赖dll

## Warning

打包文件直接使用ItemGroup而不是全部打包然后根据buildIgnore无视
若需要包含特定文件，需要在csproj里面加入<ResourceFile Include="xxx" />

  