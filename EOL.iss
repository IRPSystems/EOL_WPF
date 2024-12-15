#include "Infrastructure\Inno Setup\InstallPrerequisits.iss"
#include "Infrastructure\Inno Setup\CompareIssVersionToCurrent.iss"  

#define Major 1
#define Minor 0
#define Rev 0
#define Build 0
#define MyAppVersion Str(Major) + "." + Str(Minor)  + "." + Str(Rev)  + "." + Str(Build)


                                             
[Setup]                       
AppVersion={#MyAppVersion}
AppName=EOL
WizardStyle=modern
DefaultDirName={autopf}\EOL
DefaultGroupName=EOL
SourceDir=EOL\bin\Release\net6.0-windows
OutputDir=..\..\..\..\..\Output
OutputBaseFilename=EOLSetup


[Files]
Source: "*.*"; DestDir: "{app}"; Flags: recursesubdirs ignoreversion;
Source: "{tmp}\PEAK-System_Driver-Setup.zip"; DestDir: "{app}"; Flags: external skipifsourcedoesntexist       
Source: "{tmp}\Ixxat VCI Setup 4.0.1240.0.zip"; DestDir: "{app}"; Flags: external skipifsourcedoesntexist   
Source: "{tmp}\VCI V4.0.1240 - Windows 11, 10\Ixxat VCI Setup 4.0.1240.0.exe"; DestDir: "{app}"; Flags: external skipifsourcedoesntexist  
Source: "{tmp}\psocprogrammer_3.29.1_Windows_x86-x64.exe"; DestDir: "{app}"; Flags: external skipifsourcedoesntexist 


[Icons]
Name: "{group}\EOL" ; Filename: "{app}\EOL.exe";  Tasks: startmenu; IconFilename: {app}\Resources\EOL.ico
Name: "{commondesktop}\EOL {#MyAppVersion}"; Filename: "{app}\EOL.exe"; Tasks: desktopicon; IconFilename: {app}\Resources\EOL.ico

[Tasks]
Name: "desktopicon"; Description: "Create a desktop shortcut"; GroupDescription: "Additional icons:"; Flags: unchecked
Name: "startmenu"; Description: "Add a shortcut to the start menu"; GroupDescription: "Additional icons:"; Flags: unchecked

[Run]
Filename: {app}\EOL.exe; Description: "Launch EOL App"; Flags: postinstall skipifsilent hidewizard

[Code] 
 
var
  DownloadPage: TDownloadWizardPage;
  AppsToInstall : array of array of String;

function InitializeSetup(): Boolean;
var
  newMajor, newMinor, newRev, newBuild: Cardinal;
  tempPath : String;
Begin
  

  newMajor := {#Major};
  newMinor := {#Minor};
  newRev := {#Rev};
  newBuild := {#Build}; 
  
  Result := IsNewVersionLower_endRun(newMajor, newMinor, newRev, newBuild,'C:\Program Files (x86)\EOL\EOL.exe');

  if Result = true then
  begin
    //////////////////////////////////////////////////
    // Initiate the list of installating to do
    SetArrayLength(AppsToInstall,3);
   
    
    SetArrayLength(AppsToInstall[0], 8);
    AppsToInstall[0][0] := '{app}\PeakOemDrv.exe';  // Installation
    AppsToInstall[0][1] := '/exenoui /qb /quiet'; // Parameters       
    AppsToInstall[0][2] := 'Peak driver'; // Description    
    AppsToInstall[0][3] := ''; // Directory to search to know if already installed 
    AppsToInstall[0][4] := 'https://www.peak-system.com/quick/DrvSetup'; // Download URL
    AppsToInstall[0][5] := 'PEAK-System_Driver-Setup.zip';  // Download file   
    AppsToInstall[0][6] := '{app}\PEAK-System_Driver-Setup.zip';  // Download zip file full path         
    AppsToInstall[0][7] := 'C:\Program Files\PEAK-System';  // Path to check if the item is already installed
    
    SetArrayLength(AppsToInstall[1], 8);
    AppsToInstall[1][0] := '{app}\VCI V4.0.1240 - Windows 11, 10\Ixxat VCI Setup 4.0.1240.0.exe';  // Installation
    AppsToInstall[1][1] := '/COMPONENTS="drv/VCI4109,drv/VCI4114,core,sdk/vci3net" /SP- /VERYSILENT /NORESTART /NOCANCEL'; // Parameters       
    AppsToInstall[1][2] := 'Ixxat'; // Description    
    AppsToInstall[1][3] := ''; // Directory to search to know if already installed  
    AppsToInstall[1][4] := 'https://hmsnetworks.blob.core.windows.net/nlw/docs/default-source/products/ixxat/monitored/pc-interface-cards/vci-v4-0-1240-windows-11-10.zip?sfvrsn=2d1dfdd7_69'; // Download URL
    AppsToInstall[1][5] := 'vci-v4-0-1240-windows-11-10.zip';  // Download file    
    AppsToInstall[1][6] := '{app}\vci-v4-0-1240-windows-11-10.zip';  // Download zip file full path     
    AppsToInstall[1][7] := 'C:\Program Files (x86)\HMS\Ixxat IxAdmin';  // Path to check if the item is already installed

    SetArrayLength(AppsToInstall[2], 8);
    AppsToInstall[2][0] := '{app}\psocprogrammer_3.29.1_Windows_x86-x64.exe';  // Installation
    AppsToInstall[2][1] := '/quiet /norestart'; // Parameters       
    AppsToInstall[2][2] := 'PSoC Programmer'; // Description    
    AppsToInstall[2][3] := ''; // Directory to search to know if already installed
    AppsToInstall[2][4] := ''; // Download URL
    AppsToInstall[2][5] := '';  // Download file
    AppsToInstall[2][6] := '';  // Download zip file full path      
    AppsToInstall[2][7] := 'C:\Program Files (x86)\Cypress';  // Path to check if the item is already installed 

    
    //////////////////////////////////////////////////

    

  end;

End;



function OnDownloadProgress(const Url, FileName: String; const Progress, ProgressMax: Int64): Boolean;
begin
  
  Result := True;
end;


procedure InitializeWizard;
begin
  DownloadPage := CreateDownloadPage(SetupMessage(msgWizardPreparing), SetupMessage(msgPreparingDesc), @OnDownloadProgress);
end;

function NextButtonClick(CurPageID: Integer): Boolean;
begin
  if CurPageID = wpReady then begin
    DownloadInstall(DownloadPage, AppsToInstall);
  end;
    
  Result := True;
end;


procedure CurStepChanged(CurStep: TSetupStep);
begin
  if CurStep = ssPostInstall then
  begin
 
    RunOtherInstaller(AppsToInstall, '{app}');

    DeleteFile(ExpandConstant('{app}\PeakOemDrv.exe')); 
    DeleteFile(ExpandConstant('{app}\PEAK-System_Driver-Setup.zip'));
    DeleteFile(ExpandConstant('{app}\psocprogrammer_3.29.1_Windows_x86-x64.exe'));
    DelTree(ExpandConstant('{app}\VCI V4.0.1240 - Windows 11, 10'), True, True, True); 
    DeleteFile(ExpandConstant('{app}\param_defaults.json')); 

  end;
end;