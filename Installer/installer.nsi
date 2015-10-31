; example2.nsi
;
; This script is based on example1.nsi, but it remember the directory, 
; has uninstall support and (optionally) installs start menu shortcuts.
;
; It will install example2.nsi into a directory that the user selects,
;--------------------------------

!include "MUI.nsh"
  !define MUI_ABORTWARNING

RequestExecutionLevel user
  
; The name of the installer
Name "Fusion Engine (v0.1)"
XPStyle on

; The file to write
OutFile "FusionEngineSetup-0.1.exe"

; The default installation directory
InstallDir $PROFILE\FusionEngine

; Registry key to check for directory (so if you install again, it will 
; overwrite the old one automatically)
InstallDirRegKey HKCU "Software\FusionEngine" "Install_Dir"

; Request application privileges for Windows Vista
;RequestExecutionLevel admin

; Splash screen
Function .onInit
	# the plugins dir is automatically deleted when the installer exits
	InitPluginsDir
	File /oname=$PLUGINSDIR\splash.bmp "splash2.bmp"
	#optional
	#File /oname=$PLUGINSDIR\splash.wav "C:\myprog\sound.wav"

	splash::show 2500 $PLUGINSDIR\splash

	Pop $0 ; $0 has '1' if the user closed the splash screen early,
			; '0' if everything closed normally, and '-1' if some error occurred.
FunctionEnd


  !define MUI_HEADERIMAGE
  !define MUI_HEADERIMAGE_RIGHT
  !define MUI_HEADERIMAGE_BITMAP "header-r.bmp" ; optional
  !define MUI_LICENSEPAGE_CHECKBOX
  !define MUI_COMPONENTSPAGE_NODESC
  
  !insertmacro MUI_PAGE_LICENSE "..\LICENSE"
  !insertmacro MUI_PAGE_COMPONENTS
  !insertmacro MUI_PAGE_DIRECTORY
  !insertmacro MUI_PAGE_INSTFILES
  
  !insertmacro MUI_UNPAGE_CONFIRM
  !insertmacro MUI_UNPAGE_INSTFILES
  !insertmacro MUI_LANGUAGE "English"  
  
; Page components
; Page directory
; Page instfiles

; UninstPage uninstConfirm
; UninstPage instfiles


	
Section "Fusion Engine Core" Section1

  SectionIn RO
  
  ; Set output path to the installation directory.
  ; Binary stuff :
  SetOutPath "$INSTDIR\Bin\Release"
  
  File "..\Fusion\bin\x64\Release\*.dll"
  File "..\Fusion.Build\bin\x64\Release\*.dll"
  File "..\FBuild\bin\x64\Release\FBuild.exe"
  File "..\FScene\bin\x64\Release\*.exe"
  File "..\SDKs\FbxSdk\lib\x64\release\*.dll"
  File "..\Tools\*.dll"
  File "..\Tools\*.exe"
  File "..\Tools\*.com"

  ; Binary stuff :
  SetOutPath "$INSTDIR\Bin\Debug"
  
  File "..\Fusion\bin\x64\Debug\*.dll"
  File "..\Fusion.Build\bin\x64\Debug\*.dll"
  File "..\FBuild\bin\x64\Debug\FBuild.exe"
  File "..\FScene\bin\x64\Debug\*.exe"
  File "..\SDKs\FbxSdk\lib\x64\debug\*.dll"
  File "..\Tools\*.dll"
  File "..\Tools\*.exe"
  File "..\Tools\*.com"

  ; Content stuff :
  SetOutPath "$INSTDIR\Content"
  File "..\FusionContent\*.*"

  ; Build stuff :
  SetOutPath "$INSTDIR\Build"
  File "..\FusionProject.targets"
  
  ; Write the installation path into the registry
  WriteRegStr HKCU SOFTWARE\FusionEngine "Install_Dir" "$INSTDIR"
  
  ; Write the uninstall keys for Windows
  WriteRegStr HKCU "Software\Microsoft\Windows\CurrentVersion\Uninstall\FusionEngine" "DisplayName" "FusionEngine"
  WriteRegStr HKCU "Software\Microsoft\Windows\CurrentVersion\Uninstall\FusionEngine" "UninstallString" '"$INSTDIR\uninstall.exe"'
  WriteRegDWORD HKCU "Software\Microsoft\Windows\CurrentVersion\Uninstall\FusionEngine" "NoModify" 1
  WriteRegDWORD HKCU "Software\Microsoft\Windows\CurrentVersion\Uninstall\FusionEngine" "NoRepair" 1
  WriteUninstaller "uninstall.exe"
  
SectionEnd



Section "Fusion Engine Samples"

	SetOutPath "$INSTDIR\Samples\TestGame2"
	File /r /x bin /x obj /x Temp "..\Samples\TestGame2\*.*"

SectionEnd

  
; Section "Install DirectX End-User Runtimes (June 2010)" Section2
  ; SetOutPath "$INSTDIR\DirectX"
  ; NSISdl::download /TIMEOUT=30000 "http://download.microsoft.com/download/8/4/A/84A35BF1-DAFE-4AE8-82AF-AD2AE20B6B14/directx_Jun2010_redist.exe" "$INSTDIR\DirectX\dxsetup.exe"
  ; Pop $R0
  ; StrCmp $R0 success success
    ; SetDetailsView show
    ; DetailPrint "Download failed : $R0"
	; Abort
  ; success:
    ; ExecWait '"$INSTDIR\DirectX\dxsetup.exe" /q /t:"$INSTDIR\DirectX\Temp"'
	; ExecWait '"$INSTDIR\DirectX\Temp\DXSETUP.exe" /silent'
	; Delete   '$INSTDIR\DirectX\Temp\*.*'
	; RMDir    '$INSTDIR\DirectX\Temp'
; SectionEnd

Section "Install DirectX End-User Runtimes (June 2010)" Section2
  CreateDirectory $INSTDIR\DirectX
  File /oname=$INSTDIR\DirectX\dxsetup_jun2010.exe "dxsetup_jun2010.exe"
  ExecWait '"$INSTDIR\DirectX\dxsetup_jun2010.exe" /q /t:"$INSTDIR\DirectX\Temp"'
  ExecWait '"$INSTDIR\DirectX\Temp\DXSETUP.exe"'
  Delete   '"$INSTDIR\DirectX\Temp\*.*"'
  RMDir    '"$INSTDIR\DirectX\Temp"'
SectionEnd

  
Section "Install Visual C++ Redistributable for Visual Studio 2012" Section3
  File /oname=$PLUGINSDIR\vcredist_x64.exe "vcredist_x64.exe"
  ExecWait '$PLUGINSDIR\vcredist_x64.exe /passive'
SectionEnd


Section "Install Visual Studio Project Template"
	File /oname=$PLUGINSDIR\FusionPackage.vsix "FusionPackage.vsix"
	ExecShell "open" '$PLUGINSDIR\FusionPackage.vsix'
SectionEnd


Section "DirectX and VC++ Redistributables"
  SectionIn RO
  SetOutPath "$INSTDIR\Redist"
  File "dxsetup_jun2010.exe"
  File "vcredist_x64.exe"
SectionEnd

  
Section "Write Registry Variables"
  SectionIn RO
  WriteRegStr HKCU "Software\FusionEngine"  "BinaryDirRelease" "$INSTDIR\Bin\Release"
  WriteRegStr HKCU "Software\FusionEngine"  "BinaryDirDebug" "$INSTDIR\Bin\Debug"
  WriteRegStr HKCU "Software\FusionEngine"  "ToolsDir" "$INSTDIR\Bin\Release"
  WriteRegStr HKCU "Software\FusionEngine"  "ContentDir" "$INSTDIR\Content"
  WriteRegStr HKCU "Software\FusionEngine"  "BuildDir" "$INSTDIR\Build"
SectionEnd

;Section "Add Environment Variables"
;	ExecWait 'setx FUSION_BIN "$INSTDIR\Bin" /M'
;	ExecWait 'setx FUSION_BUILD "$INSTDIR\Build" /M'
;	ExecWait 'setx FUSION_CONTENT "$INSTDIR\Content" /M'
;SectionEnd


;--------------------------------

; Uninstaller

Section "Uninstall"
  
  ; Remove registry keys
  DeleteRegKey HKCU "Software\Microsoft\Windows\CurrentVersion\Uninstall\FusionEngine"
  DeleteRegKey HKCU SOFTWARE\FusionEngine

  DeleteRegKey HKCU "Software\FusionEngine"
  
  ; Remove files and uninstaller
  Delete $INSTDIR\Bin\Release*.*
  Delete $INSTDIR\Bin\Debug*.*
  Delete $INSTDIR\Bin\*.*
  Delete $INSTDIR\Content\*.*
  Delete $INSTDIR\Build\*.*
  Delete $INSTDIR\*.*
  RMDir /r "$INSTDIR"

  ; Remove shortcuts, if any
  ;-------!! Delete "$SMPROGRAMS\FusionEngine\*.*"
  ; Remove directories used
  ;-------!! RMDir "$SMPROGRAMS\FusionEngine"

SectionEnd
