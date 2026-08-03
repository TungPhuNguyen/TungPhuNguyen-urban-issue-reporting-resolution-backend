using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;
using UrbanIssue.Application.Common.Behaviors;
using UrbanIssue.Application.Features.Reports.CheckDuplicateReports;

namespace UrbanIssue.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(
            this IServiceCollection services)
        {
            var applicationAssembly =
                typeof(DependencyInjection)
                    .Assembly;

            services.AddMediatR(configuration =>
            {
                configuration.RegisterServicesFromAssembly(
                    typeof(DependencyInjection).Assembly);
            });

            services
                .AddValidatorsFromAssembly(
                    applicationAssembly);

            services.AddTransient(
                typeof(IPipelineBehavior<,>),
                typeof(ValidationBehavior<,>));

            services.AddScoped<
                IReportDuplicateChecker,
                ReportDuplicateChecker>();

            return services;
        }
    }
}
