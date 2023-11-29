# Migrations guide

Run all the commands mentioned below in the root of the `WirelessCom.Infrastructure.Persistence` project.

## Create a migration
```bash
dotnet ef migrations add <MigrationName>
```

## Update the database
```bash
dotnet ef database update
```

## Remove the last migration
```bash
dotnet ef migrations remove
```

## Remove the database
```bash
dotnet ef database drop
```

## List all migrations
```bash
dotnet ef migrations list
```
