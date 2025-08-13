using System.ComponentModel.DataAnnotations;

namespace CineTrackBE.Models.Attributes;

public class MaxElementsAttribute(int maxElements) : ValidationAttribute
{


    private readonly int _maxElements = maxElements;

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is IList<int> list && list.Count > _maxElements)
        {
            return new ValidationResult($"You can select up to {_maxElements} elements only.");
        }

        return  ValidationResult.Success;
    }
}
