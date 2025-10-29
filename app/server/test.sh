#!/bin/bash
export DOTNET_ROOT=/nix/store/m6b2yy0lyrwdbv7g62rpr9fw0v71x5ba-dotnet-sdk-9.0.304/share/dotnet
dotnet test "$@"
