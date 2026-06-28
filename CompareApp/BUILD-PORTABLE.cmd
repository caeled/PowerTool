@echo off
echo Building Compare portable package...
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
echo Done. Output:
echo   %~dp0publish\Compare.exe
echo.
pause
