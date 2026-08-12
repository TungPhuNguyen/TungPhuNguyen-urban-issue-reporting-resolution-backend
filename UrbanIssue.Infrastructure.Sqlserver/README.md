# UrbanIssue.Infrastructure.Sqlserver

Infrastructure layer triển khai EF Core SQL Server và các service kỹ thuật.

- `ApplicationDbContext`
- Fluent API configurations
- Migrations và development seeder
- JWT, refresh token và password hashing
- Notification và AuditLog persistence

Chạy migration từ thư mục solution:

```powershell
dotnet ef database update `
  --project UrbanIssue.Infrastructure.Sqlserver `
  --startup-project UrbanIssue.API
```
