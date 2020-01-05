#!/bin/bash
dotnet run -p ./CDN.NET.Backend &
server=$!
sleep 10
dotnet test
kill $server
# Courtesy of https://github.com/sn0w