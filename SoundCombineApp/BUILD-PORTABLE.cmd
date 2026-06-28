@echo off
echo Building SoundCombine portable package...
echo.

dotnet publish -c Release -r win-x64 ^
  -p:PublishSingleFile=true ^
  -p:SelfContained=true ^
  -p:IncludeNativeLibrariesForSelfExtract=true ^
  --output "%~dp0publish"

if errorlevel 1 (
    echo.
    echo BUILD FAILED.
    pause
    exit /b 1
)

echo.
echo Done. Portable output is in:
echo   %~dp0publish\
echo.
echo   SoundCombine.exe   -- run this
echo   bin\               -- must stay next to the exe
echo.
pause
