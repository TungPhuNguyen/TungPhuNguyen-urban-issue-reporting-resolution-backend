using MediatR;

namespace UrbanIssue.Application.Features.Areas.DeleteArea;

public sealed record DeleteAreaCommand(
    int Id)
    : IRequest<bool>;
