using MediatR;
using UrbanIssue.Application.Features.Reports.PostResolution.AutoCloseReports;

namespace UrbanIssue.API.BackgroundServices;

public sealed class AutoCloseResolvedReportsBackgroundService
    : BackgroundService
{
    private static readonly TimeSpan CheckInterval =
        TimeSpan.FromHours(1);

    private readonly IServiceScopeFactory _scopeFactory;

    private readonly ILogger<
        AutoCloseResolvedReportsBackgroundService> _logger;

    public AutoCloseResolvedReportsBackgroundService(
        IServiceScopeFactory scopeFactory,
        ILogger<AutoCloseResolvedReportsBackgroundService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessAsync(stoppingToken);
            }
            catch (OperationCanceledException)
                when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception exception)
            {
                _logger.LogError(
                    exception,
                    "Có lỗi khi tự động đóng các báo cáo Resolved.");
            }

            try
            {
                await Task.Delay(
                    CheckInterval,
                    stoppingToken);
            }
            catch (OperationCanceledException)
                when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
        }
    }

    private async Task ProcessAsync(
        CancellationToken cancellationToken)
    {
        await using var scope =
            _scopeFactory.CreateAsyncScope();

        var sender =
            scope.ServiceProvider
                .GetRequiredService<ISender>();

        var closedCount =
            await sender.Send(
                new AutoCloseResolvedReportsCommand(
                    DateTime.UtcNow),
                cancellationToken);

        if (closedCount > 0)
        {
            _logger.LogInformation(
                "Hệ thống đã tự động đóng {Count} báo cáo.",
                closedCount);
        }
    }
}
