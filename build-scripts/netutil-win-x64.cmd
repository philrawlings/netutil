@echo off

echo Powershell path:
where pwsh
IF ERRORLEVEL 1 (
    ECHO Powershell is not installed. Get it here: https://docs.microsoft.com/en-gb/powershell/scripting/install/installing-powershell
    pause
    EXIT /B
)

pwsh "netutil-win-x64.ps1"
pause