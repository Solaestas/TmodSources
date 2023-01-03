#!/bin/bash

path=${BASH_SOURCE//'\'/'/'}
path="${path%/*}"
cd "$path/Tools/EffectCompiler"

./XNBCompiler.exe $path