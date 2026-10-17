using System.ComponentModel.DataAnnotations;

namespace CofeeAdmin.Validation;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter)]
public sealed class PositivePriceAttribute : ValidationAttribute
{
    private const decimal MaximumPrice = 999999999m;

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is decimal price && price > 0m && price <= MaximumPrice)
        {
            return ValidationResult.Success;
        }

        return new ValidationResult(ErrorMessage ?? "Fiyat sıfırdan büyük ve 999999999 değerinden küçük olmalıdır.");
    }
}
