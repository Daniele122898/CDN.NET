#!/bin/sh
dotnet run -p ./CDN.NET.Backend &
server=$!
sleep 10
dotnet test
kill $server