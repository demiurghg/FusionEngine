# FusionEngine
Fusion Engine — game and scientific engine

## Setup Build Environment
Microsoft Visual Studio 2012 is required to build FusionEngine.

To build the project you need to perform the following steps:

- Add the following registry variables.  (for example user name is 'admin')
```
[HKEY_CURRENT_USER\Software\FusionEngine]
"Install_Dir"="C:\\Users\\admin\\FusionEngine"
"BinaryDirRelease"="C:\\Users\\admin\\FusionEngine\\Bin\\Release"
"BinaryDirDebug"="C:\\Users\\admin\\FusionEngine\\Bin\\Debug"
"ToolsDir"="C:\\Users\\admin\\FusionEngine\\Bin\\Release"
"ContentDir"="C:\\Users\\admin\\FusionEngine\\Content"
"BuildDir"="C:\\Users\\admin\\FusionEngine\\Build"
```

- Install [Microsoft DirectX Jun2010](https://download.microsoft.com/download/8/4/A/84A35BF1-DAFE-4AE8-82AF-AD2AE20B6B14/directx_Jun2010_redist.exe)
 
- Open FusionEngine solution. Some of the projects will not open, it's ok. Just delete them from solution.

- Build.

- Run TestGame.

