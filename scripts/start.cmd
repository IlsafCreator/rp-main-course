@echo off

cd ..\Valuator\
SET DB_RUS=localhost:6000
SET DB_EU=localhost:6001
SET DB_OTHER=localhost:6002
start dotnet run --urls "http://0.0.0.0:5001" 
start dotnet run --urls "http://0.0.0.0:5002"

cd ..\RankCalculator\
SET DB_RUS=localhost:6000
SET DB_EU=localhost:6001
SET DB_OTHER=localhost:6002
start dotnet run

cd ..\EventsLogger\
start dotnet run
start dotnet run

cd ..\nginx REM Путь к nginx
start nginx.exe -c .\conf\nginx.conf REM Путь к nginx.conf