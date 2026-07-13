using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace UrbanIssue.API.OpenApi;

public sealed class BearerSecurityOperationTransformer
    : IOpenApiOperationTransformer
{
    public Task TransformAsync(
        OpenApiOperation operation,
        OpenApiOperationTransformerContext context,
        CancellationToken cancellationToken)
    {
        var endpointMetadata =
            context.Description
                .ActionDescriptor
                .EndpointMetadata;

        var allowsAnonymous =
            endpointMetadata
                .OfType<IAllowAnonymous>()
                .Any();

        if (allowsAnonymous)
        {
            return Task.CompletedTask;
        }

        var requiresAuthorization =
            endpointMetadata
                .OfType<IAuthorizeData>()
                .Any();

        if (!requiresAuthorization)
        {
            return Task.CompletedTask;
        }

        operation.Security ??=
            [];

        operation.Security.Add(
            new OpenApiSecurityRequirement
            {
                [
                    new OpenApiSecuritySchemeReference(
                        "Bearer",
                        context.Document)
                ] = []
            });

        return Task.CompletedTask;
    }
}
