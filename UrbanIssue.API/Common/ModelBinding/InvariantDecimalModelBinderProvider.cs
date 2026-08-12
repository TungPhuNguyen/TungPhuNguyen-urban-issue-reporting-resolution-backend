using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace UrbanIssue.API.Common.ModelBinding;

public sealed class InvariantDecimalModelBinderProvider
    : IModelBinderProvider
{
    public IModelBinder? GetBinder(
        ModelBinderProviderContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var modelType = context.Metadata.ModelType;

        if (modelType == typeof(decimal)
            || modelType == typeof(decimal?))
        {
            return new InvariantDecimalModelBinder();
        }

        return null;
    }
}
