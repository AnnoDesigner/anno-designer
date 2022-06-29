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

@rem get MSBUILD_VERSION
set "MSBUILD_VERSION=%~1"
goto :MSBUILD_VERSION_CHECK
:MSBUILD_VERSION_PROMPT
set /p "MSBUILD_VERSION=Enter MSBUILD_VERSION (VS2019[7] | VS2022[10]): "
:MSBUILD_VERSION_CHECK
if "%MSBUILD_VERSION%"=="" goto :MSBUILD_VERSION_PROMPT
set MSBUILD_VERSION_RESULT=false
if "%MSBUILD_VERSION%"=="7" set MSBUILD_VERSION_RESULT=true
if "%MSBUILD_VERSION%"=="10" set MSBUILD_VERSION_RESULT=true
if "%MSBUILD_VERSION%"=="VS2019" (
set MSBUILD_VERSION_RESULT=true
set MSBUILD_VERSION=7
)
if "%MSBUILD_VERSION%"=="VS2022" (
set MSBUILD_VERSION_RESULT=true
set MSBUILD_VERSION=10
)
if "%MSBUILD_VERSION_RESULT%"=="false" (
echo not supported MSBUILD_VERSION: %MSBUILD_VERSION%
goto :MSBUILD_VERSION_PROMPT
)

@rem get USE_BINARY_LOG
set "USE_BINARY_LOG=%~1"
goto :USE_BINARY_LOG_CHECK
:USE_BINARY_LOG_PROMPT
set /p "USE_BINARY_LOG=Use binary logs? (Cause problems with paths containing spaces) (no[0] | yes[1]): "
:USE_BINARY_LOG_CHECK
if "%USE_BINARY_LOG%"=="" goto :USE_BINARY_LOG_PROMPT
set USE_BINARY_LOG_RESULT=false
if "%USE_BINARY_LOG%"=="0" set USE_BINARY_LOG_RESULT=true
if "%USE_BINARY_LOG%"=="1" set USE_BINARY_LOG_RESULT=true
if "%USE_BINARY_LOG%"=="0" (
set USE_BINARY_LOG_RESULT=true
set USE_BINARY_LOG=0
)
if "%USE_BINARY_LOG%"=="1" (
set USE_BINARY_LOG_RESULT=true
set USE_BINARY_LOG=1
)
if "%USE_BINARY_LOG_RESULT%"=="false" (
echo not supported USE_BINARY_LOG: %USE_BINARY_LOG%
goto :USE_BINARY_LOG_PROMPT
)

set BINARY_LOG=false
if "%USE_BINARY_LOG%"=="1" (
set BINARY_LOG=true
)

echo:
echo BUILD_MODE=%BUILD_MODE%
echo MSBUILD_VERSION=%MSBUILD_VERSION%
echo USE_BINARY_LOG=%BINARY_LOG%
echo:

rem  -Verbosity Diagnostic
Powershell.exe -NoProfile -ExecutionPolicy ByPass -File "%~dp0\build.ps1" --configuration %BUILD_MODE% --msbuildVersion=%MSBUILD_VERSION% --useBinaryLog=%BINARY_LOG%

:EXIT
echo finished
pause
exit