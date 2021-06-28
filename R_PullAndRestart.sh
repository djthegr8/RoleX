#!/bin/bash
echo "Checking for changes"
git pull -q
echo "Changes pulled"
pm2 stop 0 -s
echo "Starting build procedure"
dotnet build -v q
echo "Build successful"
pm2 restart 0 -s
echo "Restart successful, Good To Go!"
