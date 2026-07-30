using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;
using UrbanIssue.Application.Common.Behaviors;

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

            return services;
        }
    }
}
