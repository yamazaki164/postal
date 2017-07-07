#!/bin/bash


dotnet clean --configuration Release
dotnet lambda package --configuration Release --framework netcoreapp1.1
