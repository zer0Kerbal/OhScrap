
@echo off

rem H is the destination game folder
rem GAMEDIR is the name of the mod folder (usually the mod name)
rem GAMEDATA is the name of the local GameData
rem VERSIONFILE is the name of the version file, usually the same as GAMEDATA,
rem    but not always

set H=C:\KSP_DEV\Releases
set GAMEDIR=OhScrap
rem set SUBDIR="KermangeddonIndustries\"
set CHANGELOG="Changelog.cfg"
set README="Readme.md"
set GAMEDATA="GameData\"
set VERSIONFILE=%GAMEDIR%.version

copy /Y "%1%2" "%GAMEDATA%%GAMEDIR%\Plugins"
copy /Y %VERSIONFILE% %GAMEDATA%%SUBDIR%%GAMEDIR%
copy /Y %CHANGELOG% %GAMEDATA%%SUBDIR%%GAMEDIR%
copy /Y %README% %GAMEDATA%%SUBDIR%%GAMEDIR%

xcopy /y /s /I %GAMEDATA%%SUBDIR%%GAMEDIR% "%H%\GameData\%SUBDIR%%GAMEDIR%"

pause

rem KermangeddonIndustries\StorkDeliverySystem