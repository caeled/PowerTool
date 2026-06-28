@echo off
echo Building portable USB release...
echo This will take 3-5 minutes on first run.
echo Output: Publish\USB-Portable\SupportToolApp.exe
echo.
cd /d "%~dp0SupportToolApp"
dotnet publish /p:PublishProfile=USB-Portable
echo.
echo Done. Copy the contents of Publish\USB-Portable\ to your USB stick.
pause
