# UrbanIssue Backend — Clean Architecture v4

Khung backend cho **Hệ thống Báo cáo & Xử lý Sự cố Hạ tầng Đô thị**.

## Công nghệ định hướng

- .NET 10
- ASP.NET Core Web API
- Clean Architecture
- CQRS + MediatR
- Mapster
- Entity Framework Core 10 + SQL Server
- OpenAPI / Swagger

## Phạm vi của bộ khung

Bộ khung này chỉ tạo solution, project references, cấu trúc thư mục theo ERD v4 và phần khởi động API tối thiểu.
Chưa có entity, data model, repository, command/query handler, validator, controller nghiệp vụ, migration, JWT hay business logic.

## Các project

```text
UrbanIssue.Domain
UrbanIssue.Application
UrbanIssue.Infrastructure.Sqlserver
UrbanIssue.API
```

Xem chi tiết tại `docs/BACKEND_PROJECT_STRUCTURE.md` và ERD tại `docs/reference/ERD-v4.puml`.
# PhuTungNguyen-urban-issue-reporting-backend
