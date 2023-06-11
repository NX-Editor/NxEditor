#!/bin/bash

function iterate {
    for item in $(ls $1)
    do
        build_native "$1/$item"
    done
}

function build_native {
    if [ -d "$1/lib" ]
    then
        iterate "$1/lib"
    fi

    if [ ! -d "$1/native" ]
    then
        return
    fi

    cmake_configure "$1/native"
    cmake_build "$1/native"
}

function cmake_configure {
    mkdir --parents "$1/build/linux"
    cmake --no-warn-unused-cli -DCMAKE_EXPORT_COMPILE_COMMANDS:BOOL=TRUE -DCMAKE_BUILD_TYPE:STRING=Release -S "$1" -B "$1/build/linux" -G "Ninja"
}

function cmake_build {
    cmake --build "$1/build/linux" --config Release --target all -j 4
}

iterate "./lib"