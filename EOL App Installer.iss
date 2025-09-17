#include "Infrastructure\Inno Setup\InstallPrerequisits.iss"
#include "Infrastructure\Inno Setup\CompareIssVersionToCurrent.iss"  

#define MyAppVersion Str ("1.0.0.2")


                                             
[Setup]                       
AppVersion={#MyAppVersion}
AppName=EOL
WizardStyle=modern
DefaultDirName={autopf}\EOL
DefaultGroupName=EOL
SourceDir=EOL\bin\Release\net6.0-windows
OutputDir=C:\dev\EOL_WPF\Output
OutputBaseFilename=EOLSetup


[Files]
Source: "*.*"; DestDir: "{app}"; Flags: recursesubdirs ignoreversion;

[Icons]
Name: "{group}\EOL" ; Filename: "{app}\EOL.exe";  Tasks: startmenu; IconFilename: {app}\Resources\EOL.ico
Name: "{commondesktop}\EOL {#MyAppVersion}"; Filename: "{app}\EOL.exe"; Tasks: desktopicon; IconFilename: {app}\Resources\EOL.ico

[Tasks]
Name: "desktopicon"; Description: "Create a desktop shortcut"; GroupDescription: "Additional icons:"; Flags: unchecked
Name: "startmenu"; Description: "Add a shortcut to the start menu"; GroupDescription: "Additional icons:"; Flags: unchecked

[Run]
Filename: {app}\EOL.exe; Description: "Launch EOL App"; Flags: postinstall skipifsilent hidewizard
