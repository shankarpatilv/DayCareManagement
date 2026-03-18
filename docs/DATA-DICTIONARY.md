# Data Dictionary

<!-- markdownlint-disable MD060 -->

This document includes both current persistence constraints and planned validation behavior.

- Implemented in DB (EF Core + migrations): primary keys, foreign keys, required columns, decimal precision for GPA, and unique indexes on student/teacher email.
- Planned in importer/application layer: parsing/normalization rules and business validations (date format parsing, numeric ranges, conditional field rules, and email normalization policy).

## Student

| Column       | Type         | Required | Description/Validation                                                                                       |
| ------------ | ------------ | -------- | ------------------------------------------------------------------------------------------------------------ |
| FirstName    | string       | Yes      | Required in DB; trim/non-empty/length rules are importer/app expectations                                    |
| LastName     | string       | Yes      | Required in DB; trim/non-empty/length rules are importer/app expectations                                    |
| RegisterDate | date         | Yes      | Stored as date in DB; `dd/MM/yyyy` parsing is importer/app validation                                        |
| StudentId    | int          | Yes      | Primary key in DB; positive-range validation is importer/app expectation                                     |
| AgeMonths    | int          | Yes      | Required in DB; range `0..120` is importer/app validation                                                    |
| FatherName   | string       | Yes      | Required in DB; trim/non-empty/length rules are importer/app expectations                                    |
| MotherName   | string       | Yes      | Required in DB; trim/non-empty/length rules are importer/app expectations                                    |
| Address      | string       | Yes      | Required in DB; trim/length rules are importer/app expectations                                              |
| PhoneNo      | string       | Yes      | Required in DB; normalization and length checks are importer/app validations                                 |
| GPA          | decimal(3,2) | Yes      | Required with DB precision `(3,2)`; business range `0.00..4.00` is importer/app validation                   |
| Email        | string       | Yes      | Required with DB unique index; lowercase normalization and strict format checks are importer/app validations |
| Password     | string       | Yes      | Required in DB; trim/non-empty policy is importer/app validation                                             |

## Teacher

| Column        | Type   | Required    | Description/Validation                                                                                       |
| ------------- | ------ | ----------- | ------------------------------------------------------------------------------------------------------------ |
| FirstName     | string | Yes         | Required in DB; trim/non-empty/length rules are importer/app expectations                                    |
| LastName      | string | Yes         | Required in DB; trim/non-empty/length rules are importer/app expectations                                    |
| RegisterDate  | date   | Yes         | Stored as date in DB; `dd/MM/yyyy` parsing is importer/app validation                                        |
| TeacherId     | int    | Yes         | Primary key in DB; positive-range validation is importer/app expectation                                     |
| IsAssigned    | bool   | Yes         | Required in DB; accepted input normalization is importer/app validation                                      |
| ClassRoomName | string | Conditional | DB nullability currently allows null; conditional requirement is planned importer/app business rule          |
| Email         | string | Yes         | Required with DB unique index; lowercase normalization and strict format checks are importer/app validations |
| Password      | string | Yes         | Required in DB; trim/non-empty policy is importer/app validation                                             |
| Credits       | int    | Yes         | Required in DB; value range checks are importer/app validation                                               |

## Immunization

| Column           | Type   | Required | Description/Validation                                                               |
| ---------------- | ------ | -------- | ------------------------------------------------------------------------------------ |
| StudentId        | int    | Yes      | Required in DB with FK to student; positive-range checks are importer/app validation |
| ImmunizationId   | int    | Yes      | Required in DB; positive-range checks are importer/app validation                    |
| ImmunizationName | string | Yes      | Required in DB; trim/non-empty rules are importer/app expectations                   |
| Duration         | string | Yes      | Required in DB; normalization rules are importer/app validation                      |
| ImmunizationDate | date   | Yes      | Stored as date in DB; `dd/MM/yyyy` parsing is importer/app validation                |
| Status           | bool   | Yes      | Required in DB; accepted input normalization is importer/app validation              |

## State Rule

| Column          | Type   | Required | Description/Validation                                                       |
| --------------- | ------ | -------- | ---------------------------------------------------------------------------- |
| VaccineName     | string | Yes      | Required in DB; normalization rules are importer/app expectations            |
| DoseRequirement | string | Yes      | Required in DB; dose parsing/range interpretation is importer/app validation |
| AgeMonths       | int    | Yes      | Required in DB; value-range checks are importer/app validation               |

## Cross-entity constraints

- Currently enforced in DB: primary keys (`Student.StudentId`, `Teacher.TeacherId`), foreign key (`Immunization.StudentId` -> `Student.StudentId`), immunization uniqueness on `(StudentId, ImmunizationId, ImmunizationDate)`, and unique indexes on `Student.Email` and `Teacher.Email`.
- Planned in importer/application layer: case-insensitive email normalization semantics and date input parsing conventions (for example, `dd/MM/yyyy`).

<!-- markdownlint-enable MD060 -->
