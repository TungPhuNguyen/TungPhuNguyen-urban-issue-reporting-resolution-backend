# UrbanIssue.Application

Application layer chứa use case CQRS/MediatR, validator và abstraction.

- Command/query theo từng feature.
- FluentValidation qua pipeline behavior.
- Không phụ thuộc ASP.NET Core presentation.
- Persistence, authentication, storage, notification và audit được truy cập qua interface.
