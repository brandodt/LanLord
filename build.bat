@echo off
setlocal

set PROJECT=LanLord\LanLord.csproj
set PUBLISH_DIR=LanLord\bin\Publish
set ISS=Setup\LanLord.iss
set ISCC="C:\Program Files (x86)\Inno Setup 6\ISCC.exe"

echo ============================================================
echo  LanLord Build + Installer
echo ============================================================

:: ── Step 1: Publish (self-contained, win-x86, Release) ──────────────────────
echo.
echo [1/2] Publishing (self-contained, win-x86)...
dotnet publish "%PROJECT%" ^
  -c Release ^
  -r win-x86 ^
  --self-contained true ^
  -o "%PUBLISH_DIR%" ^
  --nologo

if %ERRORLEVEL% neq 0 (
    echo.
    echo ERROR: dotnet publish failed.
    pause
    exit /b %ERRORLEVEL%
)

:: ── Step 2: Compile Inno Setup installer ────────────────────────────────────
echo.
echo [2/2] Compiling installer...

if not exist %ISCC% (
    echo ERROR: Inno Setup 6 not found at %ISCC%.
    echo.
    echo Download it from: https://jrsoftware.org/isdl.php
    echo Then run build.bat again.
    pause
    exit /b 1
)

%ISCC% "%ISS%"

if %ERRORLEVEL% neq 0 (
    echo.
    echo ERROR: Inno Setup compilation failed.
    pause
    exit /b %ERRORLEVEL%
)

echo.
echo ============================================================
echo  SUCCESS
echo  Installer: Setup\Output\LanLord_Setup_4.exe
echo ============================================================
pause
