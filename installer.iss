; -- installer.iss --
#define MyAppName "Mikoto"
#define MyAppURL "https://github.com/liscs/Mikoto"
#define MyAppExeName "Mikoto.exe"

; 架构由 GitHub Actions 传入：/DRuntime=x64|x86|arm64
#ifndef Runtime
  #define Runtime "x64"
#endif

#define ExeFile "Mikoto\bin\Release\win-" + Runtime + "\publish\" + MyAppExeName
#define ExeFileDir "Mikoto\bin\Release\win-" + Runtime + "\publish"

#define MyAppVersion GetFileVersion(ExeFile)

#define OutputBaseFilename "Mikoto-" + MyAppVersion + "-" + Runtime + "-setup"

[Setup]
AppId={34D9A21D-0436-40BE-81B3-DFA48D35D9A9}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppVerName={#MyAppName} {#MyAppVersion}
AppPublisherURL={#MyAppURL}
AppSupportURL={#MyAppURL}
AppUpdatesURL={#MyAppURL}
ArchitecturesInstallIn64BitMode=x64compatible or arm64
DefaultDirName={autopf}\{#MyAppName}
DisableProgramGroupPage=yes
LicenseFile=LICENSE
OutputDir=Publish
OutputBaseFilename={#OutputBaseFilename}
SetupIconFile=Mikoto\logo.ico
Compression=lzma2
SolidCompression=yes
WizardStyle=modern
UninstallDisplayIcon={app}\{#MyAppExeName}
VersionInfoVersion={#MyAppVersion}


[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"
Name: "japanese"; MessagesFile: "compiler:Languages\Japanese.isl"
;Name: "chinese"; MessagesFile: "compiler:Languages\ChineseSimplified.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked
Name: "startup"; Description: "Run {#MyAppName} at Windows startup"; GroupDescription: "{cm:AdditionalIcons}"

[Files]
Source: "{#ExeFile}"; DestDir: "{app}"; Flags: ignoreversion
Source: "{#ExeFileDir}\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs
Source: "{#ExeFileDir}\data"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs onlyifdoesntexist

; 如果你还有其他共享文件（比如 README、配置文件模板等）
; Source: "README.md"; DestDir: "{app}"; Flags: isreadme

[Icons]
Name: "{autoprograms}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[Run]
; 开头启动
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(MyAppName, '&', '&&')}}"; Flags: nowait postinstall skipifsilent runascurrentuser

