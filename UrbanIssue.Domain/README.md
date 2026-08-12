# UrbanIssue.Domain

Domain layer chứa entity và enum nghiệp vụ, không phụ thuộc project khác.

Các nhóm chính:

- Identity: `Role`, `User`, `RefreshToken`
- Catalog: `Category`, `Area`, `Department`, `RoutingRule`, `SLAConfig`
- Report lifecycle: `Report`, `ReportImage`, `StatusUpdate`, `StatusUpdateImage`
- Interaction: `Upvote`, `Comment`, `Notification`, `AuditLog`
