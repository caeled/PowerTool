dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -o publish\win
echo Done: publish\win\PowellTecTools.exe