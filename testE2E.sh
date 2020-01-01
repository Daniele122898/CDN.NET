#!/bin/sh
cp ./CDN.NET.Backend/appsettings.json.example ./CDN.NET.Backend/appsettings.json
dotnet run -p ./CDN.NET.Backend &
server=$!
sleep 10
dotnet test
kill $server