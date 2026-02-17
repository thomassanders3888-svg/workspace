#!/bin/bash
# TerraForge Build Script
OUTPUT=./build
mkdir -p $OUTPUT/{windows,macos,linux}
echo "Building TerraForge..."
dotnet publish -c Release -r win-x64 -o $OUTPUT/windows
dotnet publish -c Release -r osx-x64 -o $OUTPUT/macos
echo "Build complete!"
