@echo off
title "Pdf Image Toolkit: offline - Install MSIX"
echo =========================================================================
echo   Installing 'Pdf Image Toolkit: offline' MSIX Package on Windows
echo =========================================================================
echo.

cd /d "%~dp0"

powershell -NoProfile -ExecutionPolicy Bypass -Command ^
  "Write-Host 'Checking previous installation...' -ForegroundColor Cyan; " ^
  "Get-AppxPackage -Name *PdfImageToolkit* -ErrorAction SilentlyContinue | Remove-AppxPackage -ErrorAction SilentlyContinue; " ^
  "Write-Host 'Installing MSIX package...' -ForegroundColor Cyan; " ^
  "Add-AppxPackage -Path '.\PdfImageToolkit.App_1.0.0.0_x64.msix' -ForceApplicationShutdown; " ^
  "if ($?) { " ^
  "    Write-Host 'Successfully installed!' -ForegroundColor Green; " ^
  "    Write-Host 'Launching Pdf Image Toolkit: offline...' -ForegroundColor Green; " ^
  "    Start-Process 'explorer.exe' 'shell:AppsFolder\(Get-AppxPackage -Name *PdfImageToolkit*).PackageFamilyName!App'; " ^
  "} else { " ^
  "    Write-Host 'Installation encountered an error. Check Developer Mode or certificate.' -ForegroundColor Red; " ^
  "}"

echo.
echo =========================================================================
echo   Package is also available in your Windows Start Menu!
echo =========================================================================
pause
