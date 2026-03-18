# P9 Smoke Tests

Purpose: Validate the implemented migration scope quickly before release.

## Preconditions

- API host running: `dotnet run --project src/DayCareManagement.WebApi/DayCareManagement.WebApi.csproj`
- WebApp host running: `dotnet run --project src/DayCareManagement.WebApp/DayCareManagement.WebApp.csproj`
- Test database migrated and seeded.
- Valid teacher credentials available.

## API Smoke Tests

Base URL examples assume `http://localhost:5125`.

1. **Auth login and identity**
   - `POST /auth/login` with valid teacher credentials.
   - Expected: `200 OK`, JWT token returned.
   - `GET /auth/me` with bearer token.
   - Expected: `200 OK`, identity payload includes teacher role.

2. **Authorization guard**
   - `GET /auth/teacher-only` with bearer token.
   - Expected: `200 OK` for teacher token.
   - Repeat with missing token.
   - Expected: `401 Unauthorized`.

3. **Students CRUD**
   - `GET /students`.
   - Expected: `200 OK`, list response.
   - `POST /students` with valid payload.
   - Expected: `201/200` success and student appears in subsequent `GET /students/{studentId}`.
   - `PUT /students/{studentId}` update one field.
     Expected: `204 NoContent`.
   - `DELETE /students/{studentId}` for test record.
   - Expected: success status and record no longer retrievable.

4. **Immunizations CRUD (student details path)**
   - `GET /students/{studentId}/immunizations`.
   - Expected: `200 OK`.
   - `POST /students/{studentId}/immunizations` with valid immunization.
   - Expected: success and item appears in list.
   - `PUT /students/{studentId}/immunizations/{immunizationId}/{immunizationDate}`.
   - Expected: success and list reflects update.
   - `DELETE /students/{studentId}/immunizations/{immunizationId}/{immunizationDate}`.
   - Expected: success and item removed.

5. **Teachers CRUD**
   - `GET /teachers` and `GET /teachers/{teacherId}`.
   - Expected: `200 OK` responses.
   - `POST /teachers`, `PUT /teachers/{teacherId}`, `DELETE /teachers/{teacherId}` on test record.
   - Expected: successful lifecycle without server error.

6. **Renewals and state rules**
   - `GET /renewals/due`.
   - Expected: `200 OK`, due-list payload.
   - `POST /renewals/{studentId}` for an eligible student.
   - Expected: success and due entry updated/cleared.
   - `GET /state-rules`.
   - Expected: `200 OK` with configured rules list.

## WebApp Smoke Tests

Base URL examples assume `http://localhost:5115`.

1. **Login flow**
   - Navigate to `/login` and sign in with teacher account.
   - Expected: authenticated navigation succeeds and protected pages are accessible.

2. **Students page**
   - Open students list page.
   - Expected: list renders without load error.
   - Create, edit, and delete a test student.
   - Expected: each action succeeds and UI state refreshes correctly.

3. **Student details and immunizations**
   - Open a student details page.
   - Expected: immunization list loads.
   - Add, edit, and delete one immunization entry.
   - Expected: row updates persist and no stale UI errors appear.

4. **Teachers page**
   - Open teachers list page and execute create/edit/delete on a test record.
   - Expected: successful operations and updated grid state.

5. **Renewals and state rules pages**
   - Open renewals due page and apply renewal to a due student.
   - Expected: item is processed and feedback is shown.
   - Open state-rules page and use age filter.
   - Expected: filtered list updates as expected.

6. **Session and guard checks**
   - Attempt to navigate directly to protected route after logout/session clear.
   - Expected: redirect to login or unauthorized handling.

## Execution Record Template

Runtime note (2026-03-18): local .NET 8 ASP.NET runtime blocker is resolved; release build/test/migration checks now pass.

| Area   | Step                   | Result (Pass/Fail/Blocked) | Evidence |
| ------ | ---------------------- | -------------------------- | -------- |
| API    | Auth                   | TBD                        | TBD      |
| API    | Students/Immunizations | TBD                        | TBD      |
| API    | Teachers               | TBD                        | TBD      |
| API    | Renewals/State Rules   | TBD                        | TBD      |
| WebApp | Login/Guards           | TBD                        | TBD      |
| WebApp | CRUD flows             | TBD                        | TBD      |
