#define AppVersion "1.0.0-rc4"
[Setup]
AppId={{E687BEA4-F704-4626-AC9A-36AC445B7DF2}
AppName=CDG AuditPilot
AppVersion={#AppVersion}
AppPublisher=Cap Digital Gestion
DefaultDirName={localappdata}\Programs\CDG AuditPilot
DefaultGroupName=CDG AuditPilot
PrivilegesRequired=lowest
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
OutputDir=..\artifacts\installer
OutputBaseFilename=CDG-AuditPilot-Setup-x64
Compression=lzma2
SolidCompression=yes
WizardStyle=modern
UninstallDisplayIcon={app}\CDG.AuditPilot.Desktop.exe
[Languages]
Name: "french"; MessagesFile: "compiler:Languages\French.isl"
[Files]
Source: "..\artifacts\win-x64\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs
[Icons]
Name: "{group}\CDG AuditPilot"; Filename: "{app}\CDG.AuditPilot.Desktop.exe"
Name: "{group}\Guide utilisateur"; Filename: "{app}\docs\guide-utilisateur.md"
[Run]
Filename: "{app}\CDG.AuditPilot.Desktop.exe"; Description: "Ouvrir CDG AuditPilot"; Flags: nowait postinstall skipifsilent
; Les données %LOCALAPPDATA%\CapDigitalGestion\AuditPilot sont conservées à la désinstallation.
