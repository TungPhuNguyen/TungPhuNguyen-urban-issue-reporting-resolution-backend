using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace UrbanIssue.Application.Features.Auth.Register
{
    public sealed record RegisterCommand(
    string FullName,
    string Email,
    string Password,
    string ConfirmPassword,
    string? PhoneNumber)
    : IRequest<RegisterResult>;
}
