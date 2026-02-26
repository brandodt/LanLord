; LanLord Installer Script
; Requires Inno Setup 6.x — https://jrsoftware.org/isinfo.php
;
; Usage: run build.bat — it calls `dotnet publish` then compiles this script with ISCC.exe.
; The compiled installer is written to Setup\Output\LanLord_Setup_4.exe

#define AppName      "LanLord"
#define AppVersion   "4.0.2"
#define AppPublisher "dwaynelifter"
#define AppURL       "https://github.com/dwaynelifter/LanLord"
#define AppExeName   "LanLord.exe"
#define PublishDir   "..\LanLord\bin\Publish"

; ── [Setup] ─────────────────────────────────────────────────────────────────
[Setup]
; Stable AppId — do NOT change this after first release or uninstall will break
AppId={{5A3B9E2F-4C7D-48A1-B293-D1E5F6A87C40}
AppName={#AppName}
AppVersion={#AppVersion}
AppVerName={#AppName} v{#AppVersion}
AppPublisher={#AppPublisher}
AppPublisherURL={#AppURL}
AppSupportURL={#AppURL}
AppUpdatesURL={#AppURL}

DefaultDirName={autopf}\{#AppPublisher}\{#AppName}
DefaultGroupName={#AppPublisher}\{#AppName}
DisableProgramGroupPage=yes

OutputDir=Output
OutputBaseFilename=LanLord_Setup_{#AppVersion}
Compression=lzma2
SolidCompression=yes
WizardStyle=modern

; Admin required: WinPcap driver install + Program Files write
PrivilegesRequired=admin

; Always install as 32-bit — PcapNet.dll and WinPcap are x86 only
; {autopf} resolves to Program Files (x86) when ArchitecturesInstallIn64BitMode is empty
ArchitecturesAllowed=x86 x64compatible arm64
ArchitecturesInstallIn64BitMode=

; Minimum: Windows 10 (Npcap 1.87 requires Win10+)
MinVersion=10.0

SetupIconFile=..\LanLord\Resources\dwaynelifter_icon.ico
UninstallDisplayIcon={app}\{#AppExeName}
UninstallDisplayName={#AppName} v{#AppVersion}

; ── [Languages] ─────────────────────────────────────────────────────────────
[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

; ── [Tasks] ─────────────────────────────────────────────────────────────────
[Tasks]
; No GroupDescription — keeps installnpcap at index 0 for the auto-detect code below
Name: "installnpcap"; \
  Description: "Install Npcap 1.87 (WinPcap-compatible packet capture driver)"
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; \
  GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

; ── [Files] ─────────────────────────────────────────────────────────────────
[Files]
; Self-contained .NET 8 application (all runtime DLLs, app DLLs, and content files
; such as Computerfont.ttf, npf.sys, standards-oui.ieee.org.txt)
Source: "{#PublishDir}\*"; \
  DestDir: "{app}"; \
  Flags: ignoreversion recursesubdirs createallsubdirs

; Npcap 1.87 installer — only extracted when the task is checked
Source: "..\LanLord\Resources\npcap-1.87.exe"; \
  DestDir: "{tmp}"; \
  Flags: deleteafterinstall; \
  Tasks: installnpcap

; ── [Icons] ─────────────────────────────────────────────────────────────────
[Icons]
Name: "{group}\{#AppName}";           Filename: "{app}\{#AppExeName}"
Name: "{group}\Uninstall {#AppName}"; Filename: "{uninstallexe}"
Name: "{autodesktop}\{#AppName}";     Filename: "{app}\{#AppExeName}"; \
  Tasks: desktopicon

; ── [Run] ────────────────────────────────────────────────────────────────────
[Run]
; Install Npcap 1.87 in WinPcap API-compatible mode.
; Silent install (/S) requires Npcap OEM — use the standard UI installer instead.
Filename: "{tmp}\npcap-1.87.exe"; \
  Parameters: "/winpcap-mode"; \
  StatusMsg: "Installing Npcap 1.87 (packet capture driver)..."; \
  Flags: waituntilterminated; \
  Tasks: installnpcap

; Offer to launch the app once setup completes
; shellexec: uses ShellExecuteEx so the app's requireAdministrator manifest is honoured (avoids error 740)
Filename: "{app}\{#AppExeName}"; \
  Description: "Launch {#AppName}"; \
  Flags: nowait postinstall skipifsilent shellexec

; ── [Code] ──────────────────────────────────────────────────────────────────
[Code]
// Returns True when any version of Npcap is already installed on this machine.
function NpcapIsInstalled: Boolean;
begin
  Result := RegKeyExists(HKLM, 'SOFTWARE\WOW6432Node\Npcap') or
            RegKeyExists(HKLM, 'SOFTWARE\Npcap');
end;

// Auto-uncheck "Install Npcap" when Npcap is already present so the user
// is not prompted to reinstall a driver they already have.
// installnpcap has no GroupDescription so it is always index 0 in the list.
procedure CurPageChanged(CurPageID: Integer);
begin
  if (CurPageID = wpSelectTasks) and NpcapIsInstalled then
    WizardForm.TasksList.Checked[0] := False;
end;

// ── [UninstallDelete] ────────────────────────────────────────────────────────
[UninstallDelete]
; Clean up files the app generates at runtime
Type: files; Name: "{app}\LicenseYouAccepted.txt"
