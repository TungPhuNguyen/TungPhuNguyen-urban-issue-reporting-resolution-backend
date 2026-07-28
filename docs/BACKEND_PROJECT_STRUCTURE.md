# Backend Project Structure

## 1. Dependency direction

```text
UrbanIssue.API
  -> UrbanIssue.Application
  -> UrbanIssue.Infrastructure.Sqlserver

UrbanIssue.Application
  -> UrbanIssue.Domain

UrbanIssue.Infrastructure.Sqlserver
  -> UrbanIssue.Application
  -> UrbanIssue.Domain

UrbanIssue.Domain
  -> no project dependency
```

## 2. UrbanIssue.Domain

Chứa entity và enum nghiệp vụ.

Các entity chính:

```text
Role, User, RefreshToken
Category, Area, Department, RoutingRule, SLAConfig
Report, ReportImage, StatusUpdate, StatusUpdateImage
Upvote, Comment, Notification, AuditLog
```

`Report` lưu trạng thái, phân công, SLA snapshot, khiếu nại, từ chối, mở lại và thời điểm hoàn tất.

## 3. UrbanIssue.Application

Tổ chức theo feature/use case và sử dụng MediatR.

```text
Features/
  Auth/
  Categories/
  Areas/
  Departments/
  RoutingRules/
  SlaConfigs/
  Reports/
  Notifications/
  AuditLogs/
  Dashboard/
  Users/
```

Các nhóm Report:

```text
CreateReport
CheckDuplicateReports
GetMyReports / GetMyReportById
GetReportTimeline
Upvotes / Comments
Staff: Accept, StartProcessing, ProgressNote,
       ProgressImages, Resolve, Dashboard
Admin: Assign, Reassign, Reject
PostResolution: Complaint, Close, Reopen,
                DismissComplaint, AutoClose
SlaMonitoring
Public
```

Validation dùng FluentValidation và chạy qua `ValidationBehavior`.

## 4. UrbanIssue.Infrastructure.Sqlserver

Chứa:

- `ApplicationDbContext`
- Entity configurations
- EF Core migrations
- Development seed data
- JWT/password/refresh-token services
- AuditLog và Notification services

Các enum Report được persist dưới dạng string.

## 5. UrbanIssue.API

Controller được chia theo role:

```text
Controllers/V1/Auth
Controllers/V1/Public
Controllers/V1/Citizen
Controllers/V1/Staff
Controllers/V1/Admin
```

API chịu trách nhiệm:

- Authentication/authorization
- Request binding
- Multipart upload
- OpenAPI/Swagger
- CORS và static file serving
- Background services cho SLA và auto-close
- Global ProblemDetails exception handling

## 6. Timeline

`StatusUpdate` là nguồn timeline thống nhất cho Citizen, Staff và Admin.

Mỗi timeline item trả:

```text
Id
OldStatus
NewStatus
Note
UpdatedByUserId
UpdatedByUserName
CreatedAt
ImageUrls
```

Phân quyền:

- Citizen chỉ xem Report của mình.
- Staff xem Report thuộc Department và tuân thủ quyền nhận việc.
- Admin xem toàn hệ thống.

## 7. Quy tắc repository

Không đưa vào source archive:

```text
.git/
.vs/
**/bin/
**/obj/
*.user
*.suo
*.patch
TestResults/
Runtime uploads
Production secrets
```
