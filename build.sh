#!/bin/bash
echo "Starting build process..."
dotnet restore
echo "Restore completed"
dotnet publish -c Release -o out
echo "Build completed"