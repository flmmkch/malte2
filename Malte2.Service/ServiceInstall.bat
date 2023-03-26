SET SCRIPT_DIR=%~dp0
sc create "Malte2" binpath= "%SCRIPT_DIR%Malte2.Service.exe" start= auto displayname= "Malte2 Server"