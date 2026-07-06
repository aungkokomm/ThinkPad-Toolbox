; Inno Setup script for ThinkPad Toolbox
; Builds a self-contained installer (no .NET runtime needed on the target machine),
; with an optional "run at Windows logon (as administrator)" scheduled task.

#define AppName "ThinkPad Toolbox"
#define AppVersion "1.0.1"
#define AppExe "ThinkPadToolbox.exe"
#define AppPublisher "Independent (unofficial - not affiliated with Lenovo)"
#define PublishDir "..\publish\v1.0.0"
#define IconFile "..\LEDControl\Resources\AppIcon.ico"

[Setup]
AppId={{A7F3C2E1-9B4D-4E6A-8C1F-2D5B7E9A3C4F}
AppName={#AppName}
AppVersion={#AppVersion}
AppVerName={#AppName} {#AppVersion}
AppPublisher={#AppPublisher}
DefaultDirName={autopf}\ThinkPad Toolbox
DefaultGroupName=ThinkPad Toolbox
DisableProgramGroupPage=yes
UninstallDisplayIcon={app}\{#AppExe}
UninstallDisplayName={#AppName}
OutputDir=output
OutputBaseFilename=ThinkPadToolbox-Setup-{#AppVersion}
SetupIconFile={#IconFile}
LicenseFile=..\LICENSE
Compression=lzma2/max
SolidCompression=yes
WizardStyle=modern
PrivilegesRequired=admin
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "Create a &desktop shortcut"; GroupDescription: "Shortcuts:"
Name: "startup"; Description: "Start {#AppName} automatically at Windows logon (as administrator, no UAC prompt)"; GroupDescription: "Startup:"
Name: "pawnio"; Description: "Install the PawnIO driver (needed on Windows 11 with Memory Integrity / Core Isolation enabled)"; GroupDescription: "Hardware driver:"; Flags: unchecked

[Files]
Source: "{#PublishDir}\*"; DestDir: "{app}"; Flags: recursesubdirs ignoreversion
Source: "..\LICENSE"; DestDir: "{app}"; Flags: ignoreversion
; PawnIO's signed installer, extracted to a temp dir and run only if the task is selected.
Source: "redist\PawnIO_setup.exe"; DestDir: "{tmp}"; Flags: deleteafterinstall; Tasks: pawnio

[Icons]
Name: "{group}\{#AppName}"; Filename: "{app}\{#AppExe}"
Name: "{group}\Uninstall {#AppName}"; Filename: "{uninstallexe}"
Name: "{autodesktop}\{#AppName}"; Filename: "{app}\{#AppExe}"; Tasks: desktopicon

[Run]
; Run PawnIO's own signed installer (interactive) if the user chose to install it.
Filename: "{tmp}\PawnIO_setup.exe"; StatusMsg: "Installing the PawnIO driver..."; Flags: waituntilterminated; Tasks: pawnio
; Optional: register an elevated logon task so the app starts (minimized to tray) with admin rights and no UAC prompt.
Filename: "{sys}\schtasks.exe"; Parameters: "/create /f /rl highest /sc onlogon /tn ""ThinkPad Toolbox"" /tr ""\""{app}\{#AppExe}\"" minimize"""; Flags: runhidden; Tasks: startup
; Offer to launch the app when setup finishes.
Filename: "{app}\{#AppExe}"; Description: "Launch {#AppName}"; Flags: nowait postinstall skipifsilent

[UninstallRun]
; Remove the startup task on uninstall (ignore errors if it was never created). PawnIO is
; left installed - it is a shared, harmless signed driver the user may rely on elsewhere.
Filename: "{sys}\schtasks.exe"; Parameters: "/delete /f /tn ""ThinkPad Toolbox"""; Flags: runhidden; RunOnceId: "DelStartupTask"

[Code]
{ Detect whether Windows Memory Integrity (HVCI / Core Isolation) is enabled. }
function IsMemoryIntegrityOn(): Boolean;
var
  v: Cardinal;
begin
  Result := False;
  if RegQueryDWordValue(HKLM, 'SYSTEM\CurrentControlSet\Control\DeviceGuard\Scenarios\HypervisorEnforcedCodeIntegrity', 'Enabled', v) then
    Result := (v = 1);
end;

{ When Memory Integrity is on, WinRing0 will not load, so pre-check the PawnIO task. }
procedure CurPageChanged(CurPageID: Integer);
var
  i: Integer;
begin
  if (CurPageID = wpSelectTasks) and IsMemoryIntegrityOn() then
    for i := 0 to WizardForm.TasksList.Items.Count - 1 do
      if Pos('PawnIO', WizardForm.TasksList.ItemCaption[i]) > 0 then
        WizardForm.TasksList.Checked[i] := True;
end;
