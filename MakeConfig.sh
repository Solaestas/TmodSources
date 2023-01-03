#!/bin/bash
path=${BASH_SOURCE//'\'/'/'}
path=${path%/*}
tMLPath=`$path/Tools/FindtModLoader/FindtModLoader`
echo "<Project>
    <PropertyGroup>
        <tMLPath>$tMLPath</tMLPath>
    </PropertyGroup>
</Project>" > $path/Config.targets