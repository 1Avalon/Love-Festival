@ECHO OFF

SET "source=%cd%\content_packs"
SET "target=C:\Program Files (x86)\Steam\steamapps\common\Stardew Valley\Mods\LoveFestivalContentPacks"

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