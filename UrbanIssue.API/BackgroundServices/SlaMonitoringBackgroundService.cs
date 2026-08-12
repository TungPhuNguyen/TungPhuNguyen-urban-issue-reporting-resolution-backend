using MediatR;
using UrbanIssue.Application.Features.Reports.SlaMonitoring;

namespace UrbanIssue.API.BackgroundServices;

public sealed class SlaMonitoringBackgroundService
    : BackgroundService
{
    private static readonly TimeSpan CheckInterval =
        TimeSpan.FromMinutes(5);

    private readonly IServiceScopeFactory _scopeFactory;

    private readonly ILogger<
        SlaMonitoringBackgroundService> _logger;

    public SlaMonitoringBackgroundService(
        IServiceScopeFactory scopeFactory,
        ILogger<SlaMonitoringBackgroundService> logger)
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
                await ProcessAsync(
                    stoppingToken);
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
                    "Có lỗi trong quá trình kiểm tra SLA.");
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

        var result =
            await sender.Send(
                new ProcessSlaMonitoringCommand(
                    DateTime.UtcNow),
                cancellationToken);

        if (result.WarningReportCount > 0
            || result.BreachedReportCount > 0)
        {
            _logger.LogInformation(
                "SLA monitoring hoàn tất. "
                + "Warning: {WarningCount}, "
                + "Breached: {BreachedCount}, "
                + "Escalated: {EscalatedCount}, "
                + "Notifications: {NotificationCount}.",
                result.WarningReportCount,
                result.BreachedReportCount,
                result.EscalatedReportCount,
                result.CreatedNotificationCount);
        }
    }
}
