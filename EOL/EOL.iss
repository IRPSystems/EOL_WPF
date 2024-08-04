[Setup]
AppName=EOL
AppVersion=1
WizardStyle=modern
DefaultDirName={autopf}\EOL
DefaultGroupName=EOL
SourceDir=C:\Projects\EOL_WPF\EOL\bin\Release\net6.0-windows
OutputDir=C:\Projects\EOL_WPF\EOL\Output
OutputBaseFilename=EOLSetup

[Files]
Source: "*.*"; DestDir: "{app}"; Flags: recursesubdirs ignoreversion

[Icons]
Name: "{group}\EOL"; Filename: "{app}\EOL.exe"
Name: "{commondesktop}\EOL" ; Filename: "{app}\EOL.exe"

[Code]

procedure InitializeWizard;
 Begin
 DelTree(ExpandConstant('{autopf}') + '\EOL\Data', True, True, True) ;
 
End;