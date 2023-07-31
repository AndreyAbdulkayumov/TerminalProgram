; Script generated by the Inno Setup Script Wizard.
; SEE THE DOCUMENTATION FOR DETAILS ON CREATING INNO SETUP SCRIPT FILES!

#define MyAppName "��������"


; ������ ������ ���
#define MyAppVersion "2.1.0"
#define InstallDirectory "D:\XSoft\TerminalProgram_2.1.0"
#define OutputFileName "TerminalProgram_2.1.0_installer"


#define MyAppPublisher "XSoft"
#define MyAppExeName "TerminalProgram.exe"
#define MyAppAssocName "TerminalProgram File"
#define MyAppAssocExt ".myp"
#define MyAppAssocKey StringChange(MyAppAssocName, " ", "") + MyAppAssocExt

; ������������� ����
#define PublishDirectory 'TerminalProgram\bin\Release\net7.0-windows\publish\win-x64'

#define OutputDirectory 'D:\0_Compiled_Installers\TerminalProgram'

; ������������� ����
#define LicenseFileDirectory 'LICENSE.md'

; ������������� ����
#define SetupIconFileDirectory 'TerminalProgram\Resources\MainLogo.ico'

[Setup]
; NOTE: The value of AppId uniquely identifies this application. Do not use the same AppId value in installers for other applications.
; (To generate a new GUID, click Tools | Generate GUID inside the IDE.)
AppId={{DEE0A88B-092D-4E5E-A8C3-F4F35B17E73C}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
;AppVerName={#MyAppName} {#MyAppVersion}
AppPublisher={#MyAppPublisher}
DefaultDirName={#InstallDirectory}
ChangesAssociations=yes
DefaultGroupName={#MyAppName}
AllowNoIcons=yes
LicenseFile={#LicenseFileDirectory}
; Uncomment the following line to run in non administrative install mode (install for current user only.)
;PrivilegesRequired=lowest

; ����� ������� ��������� ������������ ������ � ������� ������ ���������:
; 1. ��� ���� ��� �����
; 2. ������ ��� �������� ������������
PrivilegesRequiredOverridesAllowed=dialog

OutputDir={#OutputDirectory}\{#MyAppVersion}
OutputBaseFilename={#OutputFileName}
SetupIconFile={#SetupIconFileDirectory}
Compression=lzma
SolidCompression=yes
WizardStyle=modern

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"
Name: "russian"; MessagesFile: "compiler:Languages\Russian.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Files]
; ����������� ���� ����������
Source: "{#PublishDirectory}\{#MyAppExeName}"; DestDir: "{app}"; Flags: ignoreversion

; ��������� �����
Source: "{#PublishDirectory}\*"; DestDir: "{app}"; Flags: ignoreversion

; NOTE: Don't use "Flags: ignoreversion" on any shared system files

[Registry]
Root: HKA; Subkey: "Software\Classes\{#MyAppAssocExt}\OpenWithProgids"; ValueType: string; ValueName: "{#MyAppAssocKey}"; ValueData: ""; Flags: uninsdeletevalue
Root: HKA; Subkey: "Software\Classes\{#MyAppAssocKey}"; ValueType: string; ValueName: ""; ValueData: "{#MyAppAssocName}"; Flags: uninsdeletekey
Root: HKA; Subkey: "Software\Classes\{#MyAppAssocKey}\DefaultIcon"; ValueType: string; ValueName: ""; ValueData: "{app}\{#MyAppExeName},0"
Root: HKA; Subkey: "Software\Classes\{#MyAppAssocKey}\shell\open\command"; ValueType: string; ValueName: ""; ValueData: """{app}\{#MyAppExeName}"" ""%1"""
Root: HKA; Subkey: "Software\Classes\Applications\{#MyAppExeName}\SupportedTypes"; ValueType: string; ValueName: ".myp"; ValueData: ""

[Icons]
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{autodesktop}\{#MyAppName} {#MyAppVersion}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(MyAppName, '&', '&&')}}"; Flags: nowait postinstall skipifsilent