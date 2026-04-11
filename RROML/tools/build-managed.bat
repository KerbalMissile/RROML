@echo off
setlocal

set ROOT=%~dp0..
set CSC=%WINDIR%\Microsoft.NET\Framework64\v4.0.30319\csc.exe
if not exist "%CSC%" set CSC=%WINDIR%\Microsoft.NET\Framework\v4.0.30319\csc.exe

if not exist "%CSC%" (
    echo Could not find csc.exe
    exit /b 1
)

set OUT=%ROOT%\build\managed
if not exist "%OUT%" mkdir "%OUT%"

set FRAMEWORK64=%WINDIR%\Microsoft.NET\Framework64\v4.0.30319
set FRAMEWORK32=%WINDIR%\Microsoft.NET\Framework\v4.0.30319
set WEBEXT=%FRAMEWORK64%\System.Web.Extensions.dll
if not exist "%WEBEXT%" set WEBEXT=%FRAMEWORK32%\System.Web.Extensions.dll
set WINFORMS=%FRAMEWORK64%\System.Windows.Forms.dll
if not exist "%WINFORMS%" set WINFORMS=%FRAMEWORK32%\System.Windows.Forms.dll
set DRAWING=%FRAMEWORK64%\System.Drawing.dll
if not exist "%DRAWING%" set DRAWING=%FRAMEWORK32%\System.Drawing.dll

echo Building RROML.Abstractions.dll...
"%CSC%" /nologo /target:library /out:"%OUT%\RROML.Abstractions.dll" "%ROOT%\src\RROML.Abstractions\IRromlMod.cs"
if errorlevel 1 exit /b 1

echo Building RROML.Core.dll...
"%CSC%" /nologo /target:library /out:"%OUT%\RROML.Core.dll" /reference:"%OUT%\RROML.Abstractions.dll" /reference:"%WEBEXT%" /reference:"%WINFORMS%" /reference:"%DRAWING%" "%ROOT%\src\RROML.Core\*.cs"
if errorlevel 1 exit /b 1

echo Building ExampleMod.dll...
"%CSC%" /nologo /target:library /out:"%OUT%\ExampleMod.dll" /reference:"%OUT%\RROML.Abstractions.dll" "%ROOT%\src\ExampleMod\ExampleMod.cs"
if errorlevel 1 exit /b 1

echo Building UEBridgeMod.dll...
"%CSC%" /nologo /target:library /out:"%OUT%\UEBridgeMod.dll" /reference:"%OUT%\RROML.Abstractions.dll" /reference:"%WEBEXT%" "%ROOT%\src\UEBridgeMod\*.cs"
if errorlevel 1 exit /b 1

echo Building ModsMenuMod.dll...
"%CSC%" /nologo /target:library /out:"%OUT%\ModsMenuMod.dll" /reference:"%OUT%\RROML.Abstractions.dll" /reference:"%WEBEXT%" /reference:"%WINFORMS%" /reference:"%DRAWING%" "%ROOT%\src\ModsMenuMod\*.cs"
if errorlevel 1 exit /b 1

echo Building TimeWarpMod.dll...
"%CSC%" /nologo /target:library /out:"%OUT%\TimeWarpMod.dll" /reference:"%OUT%\RROML.Abstractions.dll" /reference:"%WEBEXT%" "%ROOT%\src\TimeWarpMod\*.cs"
if errorlevel 1 exit /b 1

echo Building MenuPauseMod.dll...
"%CSC%" /nologo /target:library /out:"%OUT%\MenuPauseMod.dll" /reference:"%OUT%\RROML.Abstractions.dll" /reference:"%WEBEXT%" "%ROOT%\src\MenuPauseMod\*.cs"
if errorlevel 1 exit /b 1

echo Building RealRailroading.dll...
"%CSC%" /nologo /target:library /out:"%OUT%\RealRailroading.dll" /reference:"%OUT%\RROML.Abstractions.dll" /reference:"%WEBEXT%" "%ROOT%\src\RealRailroading\*.cs"
if errorlevel 1 exit /b 1

echo Building BackgroundAudioMod.dll...
"%CSC%" /nologo /target:library /out:"%OUT%\BackgroundAudioMod.dll" /reference:"%OUT%\RROML.Abstractions.dll" /reference:"%WEBEXT%" "%ROOT%\src\BackgroundAudioMod\*.cs"
if errorlevel 1 exit /b 1

echo Building SlightlyBetterVisuals.dll...
"%CSC%" /nologo /target:library /out:"%OUT%\SlightlyBetterVisuals.dll" /reference:"%OUT%\RROML.Abstractions.dll" "%ROOT%\src\SlightlyBetterVisuals\*.cs"
if errorlevel 1 exit /b 1

echo Managed build finished.
exit /b 0