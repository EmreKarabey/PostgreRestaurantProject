using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Globalization;

namespace CofeeAdmin.ModelBinders
{
    public class FlexibleDecimalModelBinder : IModelBinder
    {
        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            if (bindingContext == null)
            {
                throw new ArgumentNullException(nameof(bindingContext));
            }

            var valueProviderResult = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);
            if (valueProviderResult == ValueProviderResult.None)
            {
                return Task.CompletedTask;
            }

            bindingContext.ModelState.SetModelValue(bindingContext.ModelName, valueProviderResult);

            var rawValue = valueProviderResult.FirstValue;
            if (string.IsNullOrWhiteSpace(rawValue))
            {
                return Task.CompletedTask;
            }

            var cleanValue = rawValue.Trim().Replace("₺", "").Trim();

            if (cleanValue.Contains(',') && cleanValue.Contains('.'))
            {
                if (cleanValue.IndexOf('.') < cleanValue.IndexOf(','))
                {
                    cleanValue = cleanValue.Replace(".", "").Replace(',', '.');
                }
                else
                {
                    cleanValue = cleanValue.Replace(",", "");
                }
            }
            else if (cleanValue.Contains(','))
            {
                cleanValue = cleanValue.Replace(',', '.');
            }

            if (decimal.TryParse(cleanValue, NumberStyles.Any, CultureInfo.InvariantCulture, out var result))
            {
                bindingContext.Result = ModelBindingResult.Success(result);
                return Task.CompletedTask;
            }

            bindingContext.ModelState.TryAddModelError(bindingContext.ModelName, "Lütfen geçerli bir fiyat giriniz.");
            return Task.CompletedTask;
        }
    }

    public class FlexibleDoubleModelBinder : IModelBinder
    {
        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            if (bindingContext == null)
            {
                throw new ArgumentNullException(nameof(bindingContext));
            }

            var valueProviderResult = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);
            if (valueProviderResult == ValueProviderResult.None)
            {
                return Task.CompletedTask;
            }

            bindingContext.ModelState.SetModelValue(bindingContext.ModelName, valueProviderResult);

            var rawValue = valueProviderResult.FirstValue;
            if (string.IsNullOrWhiteSpace(rawValue))
            {
                return Task.CompletedTask;
            }

            var cleanValue = rawValue.Trim();

            if (cleanValue.Contains(',') && cleanValue.Contains('.'))
            {
                if (cleanValue.IndexOf('.') < cleanValue.IndexOf(','))
                {
                    cleanValue = cleanValue.Replace(".", "").Replace(',', '.');
                }
                else
                {
                    cleanValue = cleanValue.Replace(",", "");
                }
            }
            else if (cleanValue.Contains(','))
            {
                cleanValue = cleanValue.Replace(',', '.');
            }

            if (double.TryParse(cleanValue, NumberStyles.Any, CultureInfo.InvariantCulture, out var result))
            {
                bindingContext.Result = ModelBindingResult.Success(result);
                return Task.CompletedTask;
            }

            bindingContext.ModelState.TryAddModelError(bindingContext.ModelName, "Lütfen geçerli bir sayı giriniz.");
            return Task.CompletedTask;
        }
    }

    public class FlexibleDecimalModelBinderProvider : IModelBinderProvider
    {
        public IModelBinder? GetBinder(ModelBinderProviderContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (context.Metadata.ModelType == typeof(decimal) || context.Metadata.ModelType == typeof(decimal?))
            {
                return new FlexibleDecimalModelBinder();
            }

            if (context.Metadata.ModelType == typeof(double) || context.Metadata.ModelType == typeof(double?))
            {
                return new FlexibleDoubleModelBinder();
            }

            return null;
        }
    }
}
