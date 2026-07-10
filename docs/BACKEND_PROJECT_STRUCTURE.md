# Backend Project Structure — ERD v4

## 1. Dependency direction

```text
UrbanIssue.API
  -> UrbanIssue.Application
  -> UrbanIssue.Infrastructure.Sqlserver

UrbanIssue.Application
  -> UrbanIssue.Domain

UrbanIssue.Infrastructure.Sqlserver
  -> UrbanIssue.Domain

UrbanIssue.Domain
  -> no project dependency
```

## 2. Projects

### UrbanIssue.Domain

Dành cho entity, enum, value object, domain event, repository/service abstraction và domain rule thuần.

ERD v4 hiện có 16 bảng dự kiến:

1. Role
2. User
3. RefreshToken
4. Category
5. Area
6. Department
7. RoutingRule
8. SLAConfig
9. Report
10. ReportImage
11. StatusUpdate
12. StatusUpdateImage
13. Upvote
14. Comment
15. Notification
16. AuditLog

Chưa tạo class để giữ đúng yêu cầu “chỉ tạo khung”.

### UrbanIssue.Application

Tổ chức theo feature/use case. Mỗi feature có sẵn thư mục `Commands`, `Queries`, `Validators`.

Các module chính:

- Auth
- Users / Roles
- Categories / Areas / Departments
- RoutingRules
- SlaConfigs
- Reports
- Upvotes
- Complaints
- Notifications
- Dashboards
- AuditLogs
- PublicData

Riêng `Reports` có thêm khung thư mục cho các use case quan trọng của Business Analysis v4:

- Tạo báo cáo
- Gợi ý báo cáo trùng
- Phân công thủ công
- Tiếp nhận báo cáo và áp dụng SLA
- Bắt đầu xử lý
- Thêm ghi chú tiến độ
- Đánh dấu đã xử lý kèm ảnh minh chứng
- Từ chối báo cáo
- Yêu cầu xử lý lại sau khiếu nại
- Tái phân công báo cáo bị escalation
- Lấy timeline, bản đồ công khai, báo cáo của tôi và danh sách theo đơn vị

### UrbanIssue.Infrastructure.Sqlserver

Dành cho EF Core, SQL Server, persistence model, entity configuration, repository implementation, external service và background job.

Background job đã có khung thư mục cho:

- `SlaMonitoring`: cảnh báo sắp trễ, đã trễ và escalation
- `AutoCloseReports`: tự đóng sau 7 ngày nếu không có khiếu nại hợp lệ

### UrbanIssue.API

Dành cho controller, request/response model, middleware, mapping và cấu hình presentation.

Controller được chia theo nhóm endpoint:

- Auth
- Public
- Citizen
- Staff
- Admin

## 3. Chưa được triển khai

- Entity / Enum / Value Object
- DataModel / EntityTypeConfiguration
- DbContext / Migration / Seed data
- Repository / Service
- Command / Query / Handler / Validator
- Controller nghiệp vụ
- JWT access token / refresh token
- Upload ảnh
- RoutingRule logic
- SLA logic / notification / escalation
- Auto-close / complaint / reopen flow
- Dashboard / AuditLog

## 4. Bước tiếp theo đề xuất

1. Tạo enum `ReportStatus`, `ReportPriority`, `NotificationType`, `UserRole`.
2. Tạo entity trong Domain theo ERD v4.
3. Tạo persistence model và Fluent API configuration.
4. Tạo `ApplicationDbContext`, migration đầu tiên và seed Role.
5. Làm Auth trước, sau đó CRUD danh mục, rồi mới đến Report flow.
