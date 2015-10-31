@REM -----------------------------
@REM Get registry value 
@REM http://stackoverflow.com/questions/22352793/reading-a-registry-value-to-a-batch-variable-handling-spaces-in-value
@REM -----------------------------
@FOR /F "usebackq tokens=2,* skip=2" %%L IN (
    `reg query "HKCU\Software\FusionEngine" /v Install_Dir`
) DO SET sdkpath=%%M


@echo SDK path - %sdkpath%


@echo Copy: dxsetup_jun2010.exe
@copy /Y "%sdkpath%\Redist\dxsetup_jun2010.exe" dxsetup_jun2010.exe


@echo Copy: vcredist_x64.exe
@copy /Y "%sdkpath%\Redist\vcredist_x64.exe" vcredist_x64.exe


@REM -----------------------------
@REM Call NSIS :
@REM -----------------------------
@echo Run NSIS...

@"%programfiles(x86)%\NSIS\makensis.exe" installer.nsi

@echo Done!

@del dxsetup_jun2010.exe
@del vcredist_x64.exe