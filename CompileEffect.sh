#!/bin/bash

origin=`pwd`
path=${BASH_SOURCE//'\'/'/'}
path="${path%/*}"
cd "$path/Tools/EffectCompiler"

./XNBCompiler.exe $path

cd $origin