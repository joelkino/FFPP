#!/bin/sh
dotnet ef migrations add InitialCreate_CippLogs --context CippLogs -o "./Migrations/CippLogs_Migrations"
dotnet ef migrations add InitialCreate_ExcludedTenants --context ExcludedTenants -o "./Migrations/ExcludedTenants_Migrations"
dotnet ef database update --context CippLogs
dotnet ef database update --context ExcludedTenants
