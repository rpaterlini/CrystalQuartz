cd src/.nuget
NuGet.exe install Rosalia -ExcludeVersion -OutputDirectory "../../tools"
cd "../../tools/Rosalia/tools"
Rosalia /wd="../../../src" /task=BuildPackages C:\ProjectsVS\CrystalQuartz.rpaterlini\src\CrystalQuartz.Build\bin\Debug\CrystalQuartz.Build.dll
pause