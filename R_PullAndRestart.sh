#!/bin/bash
echo "Checking for changes"
git -C /home/Hermes/RoleX pull -q
echo "Changes pulled"
pm2 stop 0 -s
echo "Starting build procedure"
dotnet build /home/Hermes/RoleX/Hermes
echo "Build successful"
pm2 restart 0 --cron "*/20 * * * *" -s
echo "Restart successful, Good To Go!"
