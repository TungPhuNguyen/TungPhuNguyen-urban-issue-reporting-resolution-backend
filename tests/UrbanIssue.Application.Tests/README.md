# UrbanIssue.Application.Tests

Bộ unit test tập trung vào Dashboard Sprint 2:

- Thống kê theo khu vực.
- Thống kê theo loại sự cố.
- Số lượng và tỷ lệ phần trăm.
- Thời gian xử lý trung bình từ `SLAStartedAt` đến `ResolvedAt`.
- Loại bỏ Report ngoài khoảng ngày.
- Trả `null` khi nhóm chưa có Report hoàn tất.
- Kiểm tra quy tắc khoảng ngày Dashboard.

Chạy toàn bộ test:

```powershell
dotnet test
```

Chạy riêng project:

```powershell
dotnet test .\tests\UrbanIssue.Application.Tests\UrbanIssue.Application.Tests.csproj
```

Thu thập coverage:

```powershell
dotnet test `
  .\tests\UrbanIssue.Application.Tests\UrbanIssue.Application.Tests.csproj `
  --collect:"XPlat Code Coverage"
```
