@echo off
setlocal

if not exist "%~dpn0.sh" echo Script "%~dpn0.sh" not found & pause & exit 2

set _CYGBIN=J:\Applications\cygwin\bin
if not exist "%_CYGBIN%" echo Couldn't find Cygwin at "%_CYGBIN%" & pause & exit 3

:: Resolve ___.sh to /cygdrive based *nix path and store in %_CYGSCRIPT%
for /f "delims=" %%A in ('%_CYGBIN%\cygpath.exe "%~dpn0.sh"') do set _CYGSCRIPT=%%A

:: Throw away temporary env vars and invoke script, passing any args that were passed to us
endlocal & %_CYGBIN%\bash --login "%_CYGSCRIPT%" %*