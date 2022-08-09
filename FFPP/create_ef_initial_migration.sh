#!/bin/sh

# Create the initial DB migrations

dotnet ef migrations add InitialCreate_FfppLogs --context FfppLogs -o "./Migrations/FfppLogs_Migrations"
dotnet ef migrations add InitialCreate_ExcludedTenants --context ExcludedTenants -o "./Migrations/ExcludedTenants_Migrations"
dotnet ef migrations add InitialCreate_UserProfiles --context UserProfiles  -o "./Migrations/UserProfiles_Migrations"

# Create the databases from latest migrations (read databases from update_databases.txt)

input="update_databases.txt"
while read -r line
do
  eval $line
done < "$input"

echo ""
echo "Script Done"
