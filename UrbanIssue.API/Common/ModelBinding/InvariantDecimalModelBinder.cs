using System.Globalization;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace UrbanIssue.API.Common.ModelBinding;

public sealed class InvariantDecimalModelBinder : IModelBinder
{
    public Task BindModelAsync(
        ModelBindingContext bindingContext)
    {
        var valueProviderResult =
            bindingContext.ValueProvider.GetValue(
                bindingContext.ModelName);

        if (valueProviderResult == ValueProviderResult.None)
        {
            return Task.CompletedTask;
        }

        bindingContext.ModelState.SetModelValue(
            bindingContext.ModelName,
            valueProviderResult);

        var value =
            valueProviderResult.FirstValue;

        if (string.IsNullOrWhiteSpace(value))
        {
            return Task.CompletedTask;
        }

        var parsed =
            decimal.TryParse(
                value,
                NumberStyles.Float,
                CultureInfo.InvariantCulture,
                out var result);

        if (!parsed)
        {
            bindingContext.ModelState.TryAddModelError(
                bindingContext.ModelName,
                $"{bindingContext.ModelName} không phải là số hợp lệ.");

            return Task.CompletedTask;
        }

        bindingContext.Result =
            ModelBindingResult.Success(result);

        return Task.CompletedTask;
    }
}
