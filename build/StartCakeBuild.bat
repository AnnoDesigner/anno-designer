@echo off

@rem %~dp0 returns current path with drive

@rem get BUILD_MODE
set "BUILD_MODE=%~1"
goto :BUILD_MODE_CHECK
:BUILD_MODE_PROMPT
set /p "BUILD_MODE=Enter BUILD_MODE (DEBUG[1] | RELEASE[2]): "
:BUILD_MODE_CHECK
if "%BUILD_MODE%"=="" goto :BUILD_MODE_PROMPT
set BUILD_MODE_RESULT=false
if "%BUILD_MODE%"=="DEBUG" set BUILD_MODE_RESULT=true
if "%BUILD_MODE%"=="RELEASE" set BUILD_MODE_RESULT=true
if "%BUILD_MODE%"=="1" (
set BUILD_MODE_RESULT=true
set BUILD_MODE=DEBUG
)
if "%BUILD_MODE%"=="2" (
set BUILD_MODE_RESULT=true
set BUILD_MODE=RELEASE
)
if "%BUILD_MODE_RESULT%"=="false" (
echo not supported BUILD_MODE: %BUILD_MODE%
goto :BUILD_MODE_PROMPT
)

echo:
echo BUILD_MODE=%BUILD_MODE%
echo:

Powershell.exe -NoProfile -ExecutionPolicy ByPass -File "%~dp0\build.ps1" -configuration %BUILD_MODE%

:EXIT
echo finished
pause
exit