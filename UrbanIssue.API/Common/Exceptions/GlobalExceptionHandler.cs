using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using UrbanIssue.Application.Common.Exceptions;

using FluentValidationException =
    FluentValidation.ValidationException;

namespace UrbanIssue.API.Common.Exceptions
{
    public sealed class GlobalExceptionHandler
    : IExceptionHandler
    {
        private readonly ILogger<GlobalExceptionHandler> _logger;

        public GlobalExceptionHandler(
            ILogger<GlobalExceptionHandler> logger)
        {
            _logger = logger;
        }

        public async ValueTask<bool> TryHandleAsync(
            HttpContext httpContext,
            Exception exception,
            CancellationToken cancellationToken)
        {
            LogException(
                exception);

            var problemDetails =
                CreateProblemDetails(
                    httpContext,
                    exception);

            httpContext.Response.StatusCode =
                problemDetails.Status
                ?? StatusCodes.Status500InternalServerError;

            await httpContext.Response.WriteAsJsonAsync(
                problemDetails,
                cancellationToken);

            return true;
        }

        private void LogException(
            Exception exception)
        {
            if (exception is FluentValidationException
                or ConflictException
                or KeyNotFoundException
                or UnauthorizedAccessException)
            {
                _logger.LogWarning(
                    exception,
                    "A handled application exception occurred.");

                return;
            }

            _logger.LogError(
                exception,
                "An unhandled exception occurred.");
        }

        private static ProblemDetails CreateProblemDetails(
            HttpContext httpContext,
            Exception exception)
        {
            ProblemDetails problemDetails =
                exception switch
                {
                    FluentValidationException validationException
                        => CreateValidationProblemDetails(
                            validationException),

                    ConflictException conflictException
                        => new ProblemDetails
                        {
                            Status =
                                StatusCodes.Status409Conflict,

                            Title =
                                "Dữ liệu bị xung đột.",

                            Detail =
                                conflictException.Message
                        },

                    KeyNotFoundException notFoundException
                        => new ProblemDetails
                        {
                            Status =
                                StatusCodes.Status404NotFound,

                            Title =
                                "Không tìm thấy dữ liệu.",

                            Detail =
                                notFoundException.Message
                        },

                    UnauthorizedAccessException
                        unauthorizedException
                        => new ProblemDetails
                        {
                            Status =
                                StatusCodes.Status401Unauthorized,

                            Title =
                                "Không được phép truy cập.",

                            Detail =
                                unauthorizedException.Message
                        },

                    _ => new ProblemDetails
                    {
                        Status =
                            StatusCodes
                                .Status500InternalServerError,

                        Title =
                            "Đã xảy ra lỗi hệ thống.",

                        Detail =
                            "Hệ thống không thể xử lý yêu cầu. "
                            + "Vui lòng thử lại sau."
                    }
                };

            problemDetails.Instance =
                httpContext.Request.Path;

            problemDetails.Extensions["traceId"] =
                httpContext.TraceIdentifier;

            return problemDetails;
        }

        private static ValidationProblemDetails
    CreateValidationProblemDetails(
        FluentValidationException exception)
        {
            var validationErrors =
                exception.Errors
                    .GroupBy(
                        failure =>
                            failure.PropertyName)
                    .ToDictionary(
                        group =>
                            group.Key,

                        group =>
                            group
                                .Select(
                                    failure =>
                                        failure.ErrorMessage)
                                .Distinct()
                                .ToArray());

            return new ValidationProblemDetails(
                validationErrors)
            {
                Status =
                    StatusCodes.Status400BadRequest,

                Title =
                    "Dữ liệu đầu vào không hợp lệ.",

                Detail =
                    "Một hoặc nhiều trường dữ liệu "
                    + "không đáp ứng yêu cầu."
            };
        }
    }
}
