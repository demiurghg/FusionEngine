@REM -----------------------------
@REM Get registry value 
@REM http://stackoverflow.com/questions/22352793/reading-a-registry-value-to-a-batch-variable-handling-spaces-in-value
@REM -----------------------------
@FOR /F "usebackq tokens=2,* skip=2" %%L IN (
    `reg query "HKCU\Software\FusionEngine" /v ToolsDir`
) DO SET sdkpath=%%M

@REM echo %sdkpath%

%sdkpath%\FBuild "." "."

@REM SubWCRev.exe "..\." installer.nsi installertmp.nsi
"%programfiles(x86)%\NSIS\makensis.exe" installer.nsi
