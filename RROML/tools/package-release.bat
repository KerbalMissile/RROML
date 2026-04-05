@echo off
setlocal

set ROOT=%~dp0..
set RELEASE=%ROOT%\build\release
set STAGE=%RELEASE%\stage
set BUILD_MANAGED=%ROOT%\build\managed
set BUILD_NATIVE=%ROOT%\build\native
set VERSION=1.0.1

call "%ROOT%\tools\build-managed.bat"
if errorlevel 1 exit /b 1

call "%ROOT%\tools\build-native.bat"
if errorlevel 1 exit /b 1

if exist "%STAGE%" rmdir /s /q "%STAGE%"
if not exist "%RELEASE%" mkdir "%RELEASE%"
mkdir "%STAGE%\RROML\arr\Binaries\Win64"
mkdir "%STAGE%\RROML\RROML"
mkdir "%STAGE%\TimeWarp\Mods\TimeWarpMod"
mkdir "%STAGE%\MenuPause\Mods\MenuPauseMod"
mkdir "%STAGE%\RealRailroading\Mods\RealRailroading"
mkdir "%STAGE%\BackgroundAudio\Mods\BackgroundAudioMod"

copy /Y "%BUILD_NATIVE%\XINPUT1_3.dll" "%STAGE%\RROML\arr\Binaries\Win64\XINPUT1_3.dll" >nul
copy /Y "%BUILD_MANAGED%\RROML.Core.dll" "%STAGE%\RROML\RROML\RROML.Core.dll" >nul
copy /Y "%BUILD_MANAGED%\RROML.Abstractions.dll" "%STAGE%\RROML\RROML\RROML.Abstractions.dll" >nul

copy /Y "%BUILD_MANAGED%\TimeWarpMod.dll" "%STAGE%\TimeWarp\Mods\TimeWarpMod\TimeWarpMod.dll" >nul
copy /Y "%ROOT%\src\TimeWarpMod\mod.json" "%STAGE%\TimeWarp\Mods\TimeWarpMod\mod.json" >nul

copy /Y "%BUILD_MANAGED%\MenuPauseMod.dll" "%STAGE%\MenuPause\Mods\MenuPauseMod\MenuPauseMod.dll" >nul
copy /Y "%ROOT%\src\MenuPauseMod\mod.json" "%STAGE%\MenuPause\Mods\MenuPauseMod\mod.json" >nul

copy /Y "%BUILD_MANAGED%\RealRailroading.dll" "%STAGE%\RealRailroading\Mods\RealRailroading\RealRailroading.dll" >nul
copy /Y "%ROOT%\src\RealRailroading\mod.json" "%STAGE%\RealRailroading\Mods\RealRailroading\mod.json" >nul

copy /Y "%BUILD_MANAGED%\BackgroundAudioMod.dll" "%STAGE%\BackgroundAudio\Mods\BackgroundAudioMod\BackgroundAudioMod.dll" >nul
copy /Y "%ROOT%\src\BackgroundAudioMod\mod.json" "%STAGE%\BackgroundAudio\Mods\BackgroundAudioMod\mod.json" >nul

if exist "%RELEASE%\RROML-v1.0.1.zip" del /q "%RELEASE%\RROML-v1.0.1.zip"
if exist "%RELEASE%\Timewarp-v1.0.1.zip" del /q "%RELEASE%\Timewarp-v1.0.1.zip"
if exist "%RELEASE%\Menu-Pause-v1.0.1.zip" del /q "%RELEASE%\Menu-Pause-v1.0.1.zip"
if exist "%RELEASE%\RealRailroading-v1.0.1.zip" del /q "%RELEASE%\RealRailroading-v1.0.1.zip"
if exist "%RELEASE%\Background-Audio-v1.0.1.zip" del /q "%RELEASE%\Background-Audio-v1.0.1.zip"

powershell -NoProfile -Command "Compress-Archive -Path '%STAGE%\RROML\*' -DestinationPath '%RELEASE%\RROML-v1.0.1.zip'"
if errorlevel 1 exit /b 1
powershell -NoProfile -Command "Compress-Archive -Path '%STAGE%\TimeWarp\*' -DestinationPath '%RELEASE%\Timewarp-v1.0.1.zip'"
if errorlevel 1 exit /b 1
powershell -NoProfile -Command "Compress-Archive -Path '%STAGE%\MenuPause\*' -DestinationPath '%RELEASE%\Menu-Pause-v1.0.1.zip'"
if errorlevel 1 exit /b 1
powershell -NoProfile -Command "Compress-Archive -Path '%STAGE%\RealRailroading\*' -DestinationPath '%RELEASE%\RealRailroading-v1.0.1.zip'"
if errorlevel 1 exit /b 1

powershell -NoProfile -Command "Compress-Archive -Path '%STAGE%\BackgroundAudio\*' -DestinationPath '%RELEASE%\Background-Audio-v1.0.1.zip'"
if errorlevel 1 exit /b 1

echo Release packages created in "%RELEASE%".
exit /b 0

