using System.Globalization;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace UrbanIssue.API.Common.ModelBinding;

public sealed class InvariantDecimalModelBinder : IModelBinder
{
    public Task BindModelAsync(ModelBindingContext bindingContext)
    {
        ArgumentNullException.ThrowIfNull(bindingContext);

        var valueResult = bindingContext.ValueProvider.GetValue(
            bindingContext.ModelName);

        if (valueResult == ValueProviderResult.None)
        {
            return Task.CompletedTask;
        }

        bindingContext.ModelState.SetModelValue(
            bindingContext.ModelName,
            valueResult);

        var rawValue = valueResult.FirstValue;

        if (string.IsNullOrWhiteSpace(rawValue))
        {
            if (bindingContext.ModelType == typeof(decimal?))
            {
                bindingContext.Result =
                    ModelBindingResult.Success(null);
            }

            return Task.CompletedTask;
        }

        var numberStyles =
            NumberStyles.AllowLeadingSign |
            NumberStyles.AllowDecimalPoint;

        if (decimal.TryParse(
                rawValue,
                numberStyles,
                CultureInfo.InvariantCulture,
                out var parsedValue))
        {
            bindingContext.Result =
                ModelBindingResult.Success(parsedValue);

            return Task.CompletedTask;
        }

        bindingContext.ModelState.TryAddModelError(
            bindingContext.ModelName,
            $"Giá trị '{rawValue}' không phải số thập phân hợp lệ. "
            + "Hãy dùng dấu chấm, ví dụ 21.0285.");

        return Task.CompletedTask;
    }
}
