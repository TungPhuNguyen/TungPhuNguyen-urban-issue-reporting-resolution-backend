using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace UrbanIssue.API.OpenApi;

public sealed class BearerSecuritySchemeTransformer
    : IOpenApiDocumentTransformer
{
    private readonly IAuthenticationSchemeProvider
        _authenticationSchemeProvider;

    public BearerSecuritySchemeTransformer(
        IAuthenticationSchemeProvider
            authenticationSchemeProvider)
    {
        _authenticationSchemeProvider =
            authenticationSchemeProvider;
    }

    public async Task TransformAsync(
        OpenApiDocument document,
        OpenApiDocumentTransformerContext context,
        CancellationToken cancellationToken)
    {
        var authenticationSchemes =
            await _authenticationSchemeProvider
                .GetAllSchemesAsync();

        var hasBearerScheme =
            authenticationSchemes.Any(
                scheme =>
                    scheme.Name == "Bearer");

        if (!hasBearerScheme)
        {
            return;
        }

        document.Components ??=
            new OpenApiComponents();

        document.Components.SecuritySchemes ??=
            new Dictionary<
                string,
                IOpenApiSecurityScheme>();

        document.Components
            .SecuritySchemes["Bearer"] =
                new OpenApiSecurityScheme
                {
                    Type =
                        SecuritySchemeType.Http,

                    Scheme =
                        "bearer",

                    BearerFormat =
                        "JWT",

                    In =
                        ParameterLocation.Header,

                    Description =
                        "Nhập JWT access token."
                };
    }
}
