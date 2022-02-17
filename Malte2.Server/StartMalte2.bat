tasklist /fi "ImageName eq Malte2.Server.exe" /fo csv 2>NUL | find /I "Malte2.Server.exe">NUL
if "%ERRORLEVEL%" NEQ "0" (
    start /b Malte2.Server.exe
    timeout 4
)

start "" "http://localhost:5000"