using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using UrbanIssue.Application.Common.Exceptions;
using Microsoft.EntityFrameworkCore;

using FluentValidationException =
    FluentValidation.ValidationException;

namespace UrbanIssue.API.Common.Exceptions;

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
        if (exception is FluentValidationException validationException)
        {
            await WriteValidationProblemDetailsAsync(
                httpContext,
                validationException,
                cancellationToken);

            return true;
        }

        var (statusCode, title) =
            exception switch
            {
                PotentialDuplicateException => (
                    StatusCodes.Status409Conflict,
                    "Phát hiện phản ánh có thể bị trùng."),

                DbUpdateConcurrencyException => (
                    StatusCodes.Status409Conflict,
                    "Dữ liệu vừa được người khác thay đổi."),

                ConflictException => (
                    StatusCodes.Status409Conflict,
                    "Dữ liệu bị xung đột."),

                KeyNotFoundException => (
                    StatusCodes.Status404NotFound,
                    "Không tìm thấy dữ liệu."),

                UnauthorizedAccessException => (
                    StatusCodes.Status401Unauthorized,
                    "Không được phép truy cập."),

                InvalidOperationException => (
                    StatusCodes.Status400BadRequest,
                    "Yêu cầu không hợp lệ."),

                _ => (
                    StatusCodes.Status500InternalServerError,
                    "Đã xảy ra lỗi hệ thống.")
            };

        if (statusCode >= 500)
        {
            _logger.LogError(
                exception,
                "Unhandled exception while processing {Method} {Path}",
                httpContext.Request.Method,
                httpContext.Request.Path);
        }
        else
        {
            _logger.LogWarning(
                exception,
                "Request failed with status {StatusCode}: {Method} {Path}",
                statusCode,
                httpContext.Request.Method,
                httpContext.Request.Path);
        }

        var problemDetails =
            new ProblemDetails
            {
                Status = statusCode,
                Title = title,
                Detail =
                    statusCode == StatusCodes.Status500InternalServerError
                        ? "Máy chủ không thể xử lý yêu cầu."
                        : exception.Message,
                Instance = httpContext.Request.Path
            };

        problemDetails.Extensions["traceId"] =
            httpContext.TraceIdentifier;

        if (exception is PotentialDuplicateException duplicateException)
        {
            problemDetails.Extensions["duplicates"] =
                duplicateException.Duplicates;
        }

        httpContext.Response.StatusCode =
            statusCode;

        await httpContext.Response.WriteAsJsonAsync(
            problemDetails,
            cancellationToken);

        return true;
    }

    private static async Task
        WriteValidationProblemDetailsAsync(
            HttpContext httpContext,
            FluentValidationException exception,
            CancellationToken cancellationToken)
    {
        var errors =
            exception.Errors
                .GroupBy(error => error.PropertyName)
                .ToDictionary(
                    group => group.Key,
                    group => group
                        .Select(error => error.ErrorMessage)
                        .Distinct()
                        .ToArray());

        var problemDetails =
            new ValidationProblemDetails(errors)
            {
                Status =
                    StatusCodes.Status400BadRequest,

                Title =
                    "Dữ liệu đầu vào không hợp lệ.",

                Detail =
                    "Một hoặc nhiều trường dữ liệu không đáp ứng yêu cầu.",

                Instance =
                    httpContext.Request.Path
            };

        problemDetails.Extensions["traceId"] =
            httpContext.TraceIdentifier;

        httpContext.Response.StatusCode =
            StatusCodes.Status400BadRequest;

        await httpContext.Response.WriteAsJsonAsync(
            problemDetails,
            cancellationToken);
    }
}
