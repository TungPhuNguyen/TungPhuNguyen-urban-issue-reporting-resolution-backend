# Frontend Integration

## Complaint state trong Report detail

Citizen, Staff và Admin detail API trả thêm:

```json
{
  "hasSubmittedComplaint": true,
  "complaintSubmittedAt": "2026-07-28T08:00:00Z",
  "complaintReason": "Kết quả xử lý chưa đáp ứng yêu cầu."
}
```

Ý nghĩa:

- `hasSubmittedComplaint`: Report đã từng được khiếu nại; không reset sau Reopen.
- `complaintSubmittedAt`: có khiếu nại đang chờ Admin xử lý.
- `complaintReason`: nội dung khiếu nại đang chờ.

Frontend Citizen chỉ hiện nút khiếu nại khi:

```text
status == Resolved && hasSubmittedComplaint == false
```

## Timeline theo role

```http
GET /api/v1/citizen/reports/{id}/timeline
GET /api/v1/staff/reports/{id}/timeline
GET /api/v1/admin/reports/{id}/timeline
```

Response item:

```json
{
  "id": 12,
  "oldStatus": "InProgress",
  "newStatus": "InProgress",
  "note": "Đã thay linh kiện hỏng.",
  "updatedByUserId": "...",
  "updatedByUserName": "Nguyễn Văn B",
  "createdAt": "2026-07-28T08:30:00Z",
  "imageUrls": [
    "/uploads/reports/.../progress.jpg"
  ]
}
```

## Staff progress

```http
POST /api/v1/staff/reports/{id}/progress-notes
Content-Type: application/json

{
  "note": "Đã kiểm tra hiện trường và đang chờ linh kiện."
}
```

```http
POST /api/v1/staff/reports/{id}/progress-images
Content-Type: multipart/form-data

Note: Đã thay thiết bị hỏng.
Images: progress.jpg
```

## Staff dashboard

```http
GET /api/v1/staff/dashboard/summary
GET /api/v1/staff/dashboard/summary?from=2026-07-01T00:00:00Z&to=2026-07-31T23:59:59Z
```

## Admin complaint decision

Chấp nhận và mở lại:

```http
POST /api/v1/admin/reports/{id}/reopen

{
  "reason": "Khiếu nại hợp lệ, yêu cầu kiểm tra và xử lý lại."
}
```

Không chấp nhận và đóng:

```http
POST /api/v1/admin/reports/{id}/dismiss-complaint

{
  "reason": "Kết quả xử lý đã đáp ứng yêu cầu."
}
```
