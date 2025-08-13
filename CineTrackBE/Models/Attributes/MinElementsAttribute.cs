using System.ComponentModel.DataAnnotations;

namespace CineTrackBE.Models.Attributes
{
    public class MinElementsAttribute(int minElements): ValidationAttribute
    {

        private readonly int _minElements = minElements;

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value is IList<int> list && list.Count < _minElements)
            {
                return new ValidationResult($"You must select minimum {_minElements} elements.");
            }

            return ValidationResult.Success;
        }





    }
}
