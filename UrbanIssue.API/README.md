# UrbanIssue.API

ASP.NET Core Web API cho UrbanIssue.

Development:

```powershell
dotnet run --project UrbanIssue.API
```

URLs mặc định:

```text
https://localhost:7180
http://localhost:5180
https://localhost:7180/swagger
```

Controller được phân nhóm theo `Public`, `Citizen`, `Staff`, `Admin` và bảo vệ bằng JWT role authorization.
