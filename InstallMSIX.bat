@echo off
title "Pdf Image Toolkit: offline - Install"
echo ============================================================
echo   Installing Pdf Image Toolkit: offline on Windows...
echo ============================================================
echo.

cd /d "%~dp0src\PdfImageToolkit.App\bin\x64\Release\net8.0-windows10.0.26100.0\win-x64\AppPackages\PdfImageToolkit.App_1.0.0.0_x64_Test"

powershell -NoProfile -Command "Get-AppxPackage -Name *PdfImageToolkit* -ErrorAction SilentlyContinue | Remove-AppxPackage -ErrorAction SilentlyContinue; Add-AppxPackage -Path '.\PdfImageToolkit.App_1.0.0.0_x64.msix' -ForceApplicationShutdown; if ($?) { Write-Host 'Successfully installed! Launching application...' -ForegroundColor Green; Start-Process 'explorer.exe' 'shell:AppsFolder\(Get-AppxPackage -Name *PdfImageToolkit*).PackageFamilyName!App' } else { Write-Host 'Installation failed. See error above.' -ForegroundColor Red }"

echo.
echo ============================================================
echo   Done! You can also open 'Pdf Image Toolkit: offline' from Start Menu.
echo ============================================================
pause
