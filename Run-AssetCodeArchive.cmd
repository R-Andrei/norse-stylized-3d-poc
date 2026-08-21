@echo off
setlocal
title Archive Unity Asset Code

set "SCRIPT=%~dp0Archive-AssetCode.ps1"
set "ARCHIVE=%~dp0Assets-Code-Archive.zip"

if not exist "%SCRIPT%" (
    echo ERROR: Archive-AssetCode.ps1 was not found beside this launcher.
    goto :failed
)

set "FORCE_ARGUMENT="
if exist "%ARCHIVE%" (
    echo.
    choice /C YN /N /M "Assets-Code-Archive.zip already exists. Replace it? [Y/N] "
    if errorlevel 2 goto :cancelled
    set "FORCE_ARGUMENT=-Force"
)

echo.
powershell.exe -NoProfile -ExecutionPolicy Bypass -File "%SCRIPT%" -OutputPath "%ARCHIVE%" %FORCE_ARGUMENT%
set "EXIT_CODE=%ERRORLEVEL%"

echo.
if not "%EXIT_CODE%"=="0" (
    echo Archive failed with exit code %EXIT_CODE%.
    goto :failed
)

echo Archive created successfully.
echo.
pause
exit /b 0

:cancelled
echo.
echo Cancelled. The existing archive was not changed.
echo.
pause
exit /b 0

:failed
echo.
pause
exit /b 1
