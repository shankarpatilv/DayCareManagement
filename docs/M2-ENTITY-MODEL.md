# M2 Entity Model

## Tables and Keys

| Table         | Primary Key                                                    | Notes                                                  |
| ------------- | -------------------------------------------------------------- | ------------------------------------------------------ |
| Students      | `StudentId`                                                    | Unique email index on `Email`                          |
| Teachers      | `TeacherId`                                                    | Unique email index on `Email`                          |
| Immunizations | Composite: (`StudentId`, `ImmunizationId`, `ImmunizationDate`) | FK `StudentId` -> `Students.StudentId`; cascade delete |
| StateRules    | Composite: (`VaccineName`, `DoseRequirement`, `AgeMonths`)     | State vaccine dose rules                               |

## Field Notes

- All date fields use `DateOnly` in the domain model (`RegisterDate`, `ImmunizationDate`).
- Student GPA is mapped as decimal precision `(3,2)`.
- Student and teacher IDs are configured as externally provided values (`ValueGeneratedNever`).

## Migration Commands

From solution root:

```bash
dotnet restore DayCareManagement.sln
dotnet build DayCareManagement.sln
dotnet test DayCareManagement.sln
```

If `dotnet ef` tools are installed:

```bash
dotnet ef migrations add InitialCreate \
  --project src/DayCareManagement.Infrastructure/DayCareManagement.Infrastructure.csproj \
  --startup-project src/DayCareManagement.WebApi/DayCareManagement.WebApi.csproj \
  --context DayCareManagementDbContext \
  --output-dir Persistence/Migrations
```

The PostgreSQL connection string is resolved in this order: `DAYCAREMANAGEMENT_CONNECTIONSTRING`, then `ConnectionStrings__DefaultConnection`, then `ConnectionStrings:DefaultConnection` from configuration. Use a local `.env` file (not committed) for local secrets.

Apply migration:

```bash
dotnet ef database update \
  --project src/DayCareManagement.Infrastructure/DayCareManagement.Infrastructure.csproj \
  --startup-project src/DayCareManagement.WebApi/DayCareManagement.WebApi.csproj \
  --context DayCareManagementDbContext
```
