#!/bin/bash
dotnet publish -c Release -r osx-arm64 --self-contained true -p:PublishSingleFile=true -o publish/mac
echo "Done: publish/mac/PowellTecTools"