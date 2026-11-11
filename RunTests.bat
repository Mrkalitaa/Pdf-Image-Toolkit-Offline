@echo off
title "PDF & Image Toolkit - Unit Tests"
echo ============================================================
echo   Running PDF and Image Toolkit xUnit Test Suite (29 Tests)
echo ============================================================
echo.

set "DOTNET_CMD=%USERPROFILE%\.dotnet\dotnet.exe"
if not exist "%DOTNET_CMD%" (
    set "DOTNET_CMD=dotnet"
)

"%DOTNET_CMD%" test "%~dp0tests\PdfImageToolkit.Tests\PdfImageToolkit.Tests.csproj"
echo.
echo ============================================================
echo   Test run completed. Press any key to close this window.
echo ============================================================
pause >nul
