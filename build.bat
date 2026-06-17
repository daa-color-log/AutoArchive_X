@echo off
set "CSC=C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe"
if not exist "%CSC%" (
    echo Error: C# Compiler not found at %CSC%
    pause
    exit /b 1
)

echo Compiling AutoArchive_X WPF Application...
"%CSC%" /target:winexe /win32icon:app_icon.ico /out:AutoArchive_X.exe /lib:C:\Windows\Microsoft.NET\Framework64\v4.0.30319,C:\Windows\Microsoft.NET\Framework64\v4.0.30319\WPF /r:PresentationFramework.dll /r:PresentationCore.dll /r:WindowsBase.dll /r:System.Xaml.dll /r:System.Drawing.dll /r:System.dll /r:System.Core.dll /r:System.Xml.dll /r:System.Windows.Forms.dll AppLogic.cs MainWindow.cs Program.cs

if %errorlevel% neq 0 (
    echo Compilation FAILED!
    exit /b %errorlevel%
)

echo Compilation SUCCESSFUL!
echo Output file: AutoArchive_X.exe
