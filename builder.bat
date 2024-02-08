@ECHO OFF

SET "source=C:\Users\axj30\Desktop\Coding\LoveFestival\LoveFestival\[CP] Love Festival Default Dates"
SET "target=D:\SteamLibrary\steamapps\common\Stardew Valley\mods\[CP] Love Festival Default Dates"

REM Führe robocopy aus, um nur unterschiedliche Dateien zu kopieren
robocopy "%source%" "%target%" /E /XO /L

REM Überprüfe den Exit Code von robocopy
IF %ERRORLEVEL% EQU 0 (
    ECHO Die Ordnerinhalte sind identisch. Kein Bedarf zu kopieren.
) ELSE (
    ECHO Die Ordnerinhalte sind unterschiedlich. Starte den Kopiervorgang...
    robocopy "%source%" "%target%" /E /MIR
)

PAUSE