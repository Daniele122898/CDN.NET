#!/bin/sh
mv ./CDN.NET.Backend/appsettings.json.example ./CDN.NET.Backend/appsettings.json
dotnet run -p ./CDN.NET.Backend &
server=$!
sleep 10
dotnet test
kill $server
mv ./CDN.NET.Backend/appsettings.json ./CDN.NET.Backend/appsettings.json.example