REM RUN prepare.bat

SET DST_PATH=..\src\ServiceStack.CefGlue\CefGlue

RMDIR %DST_PATH% /s /q

XCOPY /E ..\..\cefglue\CefGlue %DST_PATH%\

DEL %DST_PATH%\CefGlue.csproj %DST_PATH%\Interop\Base\cef_string_t.disabled.cs

RMDIR ..\src\ServiceStack.CefGlue.Win64\locales /s /q
RMDIR ..\src\ServiceStack.CefGlue.Win64\swiftshader /s /q
DEL ..\src\ServiceStack.CefGlue.Win64\*.pak ..\src\ServiceStack.CefGlue.Win64\*.lib ..\src\ServiceStack.CefGlue.Win64\*.dll ..\src\ServiceStack.CefGlue.Win64\*.bin ..\src\ServiceStack.CefGlue.Win64\*.dat ..\src\ServiceStack.CefGlue.Win64\*.exe

XCOPY /E C:\src\cef_binary_windows64\Release ..\src\ServiceStack.CefGlue.Win64\
XCOPY /E C:\src\cef_binary_windows64\Resources ..\src\ServiceStack.CefGlue.Win64\
COPY C:\src\cef_binary_windows64_client\Release\cefclient.exe ..\src\ServiceStack.CefGlue.Win64\
