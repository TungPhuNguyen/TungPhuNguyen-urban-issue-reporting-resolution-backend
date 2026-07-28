# Implemented items 1–5

## 1. Complaint data in detail APIs

Added to Citizen, Staff and Admin report detail responses:

```text
HasSubmittedComplaint
ComplaintSubmittedAt
ComplaintReason
```

No migration is required because these fields already exist in `Report`.

## 2. Frontend integration

The accompanying frontend archive includes:

- Admin Reopen and Dismiss Complaint actions.
- Staff Dashboard from `/staff/dashboard/summary`.
- Staff Progress Note and Progress Images actions.
- Citizen complaint button controlled by `hasSubmittedComplaint`.

## 3. Shared timeline

Added role endpoints:

```http
GET /api/v1/citizen/reports/{id}/timeline
GET /api/v1/staff/reports/{id}/timeline
GET /api/v1/admin/reports/{id}/timeline
```

Timeline now returns actor identity and progress images.

## 4. RoutingRule index

Removed the duplicated Fluent API declaration. The remaining unique key is:

```text
CategoryId + AreaId + DepartmentId
```

No migration is required because the database schema does not change.

## 5. Repository cleanup

- Removed tracked runtime report images.
- Removed obsolete patch file.
- Updated `.gitignore`.
- Updated root and project README files.
- Updated architecture and frontend integration documentation.
- Clean delivery archives exclude `.git`, `.vs`, `bin`, `obj`, `.user`, runtime uploads and temporary files.

## Validation performed

- `git diff --check`: passed.
- Static C# delimiter check: passed.
- DTO constructor argument counts: passed.
- JavaScript/JSX parsing with TypeScript compiler: passed.

The execution environment does not contain .NET SDK and could not run `dotnet build`. Run locally:

```powershell
dotnet clean
dotnet build
dotnet test
```
