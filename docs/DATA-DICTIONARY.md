# Data Dictionary

## Student

| Column       | Type         | Required | Description/Validation                      |
| ------------ | ------------ | -------- | ------------------------------------------- |
| FirstName    | string       | Yes      | Trimmed, non-empty, max 100 characters      |
| LastName     | string       | Yes      | Trimmed, non-empty, max 100 characters      |
| RegisterDate | date         | Yes      | Date in `dd/MM/yyyy` format                 |
| StudentId    | int          | Yes      | Positive integer, unique student identifier |
| AgeMonths    | int          | Yes      | Integer range `0..120`                      |
| FatherName   | string       | Yes      | Trimmed, non-empty, max 150 characters      |
| MotherName   | string       | Yes      | Trimmed, non-empty, max 150 characters      |
| Address      | string       | Yes      | Trimmed, max 300 characters                 |
| PhoneNo      | string       | Yes      | Digits after normalization, length `7..15`  |
| GPA          | decimal(3,2) | Yes      | Decimal range `0.00..4.00`                  |
| Email        | string       | Yes      | Trimmed, lowercase, valid email format      |
| Password     | string       | Yes      | Trimmed, non-empty                          |

## Teacher

| Column        | Type   | Required    | Description/Validation                                |
| ------------- | ------ | ----------- | ----------------------------------------------------- |
| FirstName     | string | Yes         | Trimmed, non-empty, max 100 characters                |
| LastName      | string | Yes         | Trimmed, non-empty, max 100 characters                |
| RegisterDate  | date   | Yes         | Date in `dd/MM/yyyy` format                           |
| TeacherId     | int    | Yes         | Positive integer, unique teacher identifier           |
| IsAssigned    | bool   | Yes         | Boolean (`true`/`false`, case-insensitive)            |
| ClassRoomName | string | Conditional | Required when `IsAssigned = true`; otherwise nullable |
| Email         | string | Yes         | Trimmed, lowercase, valid email format                |
| Password      | string | Yes         | Trimmed, non-empty                                    |
| Credits       | int    | Yes         | Integer `>= 0`                                        |

## Immunization

| Column           | Type   | Required | Description/Validation                                     |
| ---------------- | ------ | -------- | ---------------------------------------------------------- |
| StudentId        | int    | Yes      | Positive integer; must reference an existing student       |
| ImmunizationId   | int    | Yes      | Positive integer immunization identifier                   |
| ImmunizationName | string | Yes      | Trimmed, non-empty                                         |
| Duration         | string | Yes      | Normalized duration expression (for example, months/years) |
| ImmunizationDate | date   | Yes      | Date in `dd/MM/yyyy` format                                |
| Status           | bool   | Yes      | Boolean (`true`/`false`, case-insensitive)                 |

## State Rule

| Column          | Type   | Required | Description/Validation                          |
| --------------- | ------ | -------- | ----------------------------------------------- |
| VaccineName     | string | Yes      | Trimmed vaccine name                            |
| DoseRequirement | string | Yes      | Dose value or range (for example, `4` or `1-4`) |
| AgeMonths       | int    | Yes      | Integer age in months                           |

## Cross-entity constraints

- Primary keys: `Student.StudentId`, `Teacher.TeacherId`.
- Foreign key: `Immunization.StudentId` references `Student.StudentId`.
- Immunization uniqueness: enforce uniqueness on `(StudentId, ImmunizationId, ImmunizationDate)`.
- Email uniqueness: `Student.Email` and `Teacher.Email` should be unique (case-insensitive).
- Date expectation: all date fields use `dd/MM/yyyy` and should be stored as date-only where applicable.
