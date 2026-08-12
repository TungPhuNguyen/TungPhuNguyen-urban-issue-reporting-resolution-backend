# UrbanIssue Backend

Backend cho **Hệ thống Báo cáo & Xử lý Sự cố Hạ tầng Đô thị**.

## Công nghệ

- .NET 10 và ASP.NET Core Web API
- Clean Architecture
- CQRS với MediatR
- FluentValidation
- Entity Framework Core 10 và SQL Server
- JWT access token + refresh token rotation
- OpenAPI / Swagger

## Các project

```text
UrbanIssue.Domain
UrbanIssue.Application
UrbanIssue.Infrastructure.Sqlserver
UrbanIssue.API
```

Dependency direction:

```text
API -> Application
API -> Infrastructure.Sqlserver
Application -> Domain
Infrastructure.Sqlserver -> Application + Domain
```

## Phạm vi đã triển khai

### Citizen

- Đăng ký, đăng nhập, refresh token, đăng xuất.
- Tạo báo cáo kèm vị trí và ảnh.
- Gợi ý báo cáo trùng lặp theo loại sự cố và khoảng cách.
- Xem báo cáo của tôi và timeline.
- Upvote và bình luận.
- Gửi khiếu nại trong 7 ngày sau khi báo cáo được giải quyết.
- Xác nhận đóng báo cáo.
- Một báo cáo chỉ được khiếu nại một lần trong toàn bộ vòng đời.

### Staff

- Xem báo cáo thuộc đơn vị.
- Tiếp nhận và chọn mức ưu tiên.
- Bắt đầu xử lý.
- Thêm ghi chú và ảnh tiến độ.
- Hoàn tất xử lý kèm ghi chú và ảnh minh chứng.
- Xem nội dung khiếu nại liên quan.
- Xem timeline đầy đủ và dashboard của đơn vị.

### Admin

- CRUD Category, Area, Department, RoutingRule và SLAConfig.
- Quản lý Staff và trạng thái tài khoản.
- Phân công, tái phân công và từ chối báo cáo.
- Chấp nhận khiếu nại để mở lại báo cáo.
- Không chấp nhận khiếu nại và đóng báo cáo.
- Xem AuditLog, Notification và dashboard toàn hệ thống.

### Hệ thống

- Tự động định tuyến theo Category + Area.
- SLA Warning tại 80%, Breach tại 100%, Escalation tại 150%.
- Tự động đóng báo cáo sau 7 ngày nếu không có khiếu nại đang chờ.
- Public map và public report detail.
- Timeline dùng `StatusUpdate` và `StatusUpdateImage` làm nguồn dữ liệu.

## Cài đặt

### 1. Yêu cầu

- .NET SDK 10
- SQL Server
- EF Core CLI:

```powershell
dotnet tool install --global dotnet-ef
```

### 2. Connection string

Cập nhật `UrbanIssue.API/appsettings.json` hoặc dùng User Secrets:

```powershell
cd UrbanIssue.API
dotnet user-secrets init
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=localhost;Database=UrbanIssue;Trusted_Connection=True;TrustServerCertificate=True"
dotnet user-secrets set "Jwt:SecretKey" "YOUR-LOCAL-DEVELOPMENT-SECRET-KEY"
```

### 3. Migration

Từ thư mục gốc:

```powershell
dotnet ef database update `
  --project UrbanIssue.Infrastructure.Sqlserver `
  --startup-project UrbanIssue.API
```

### 4. Chạy API

```powershell
dotnet run --project UrbanIssue.API
```

Development URLs:

```text
https://localhost:7180
http://localhost:5180
https://localhost:7180/swagger
```

Development seeder được chạy khi `ASPNETCORE_ENVIRONMENT=Development`.

## Endpoint bổ sung quan trọng

```http
GET  /api/v1/citizen/reports/{id}/timeline
GET  /api/v1/staff/reports/{id}/timeline
GET  /api/v1/admin/reports/{id}/timeline

POST /api/v1/staff/reports/{id}/progress-notes
POST /api/v1/staff/reports/{id}/progress-images
GET  /api/v1/staff/dashboard/summary

POST /api/v1/admin/reports/{id}/reopen
POST /api/v1/admin/reports/{id}/dismiss-complaint
```

## Kiểm tra trước khi commit

```powershell
dotnet clean
dotnet build
dotnet test
```

Không commit `.vs`, `bin`, `obj`, file upload runtime, secret, file `.user` hoặc patch tạm.

Xem thêm:

- `docs/BACKEND_PROJECT_STRUCTURE.md`
- `docs/FRONTEND_INTEGRATION.md`
- `docs/ERD_TABLE_CHECKLIST.md`
