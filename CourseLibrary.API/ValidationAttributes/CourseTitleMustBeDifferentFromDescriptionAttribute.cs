using CourseLibrary.API.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace CourseLibrary.API.ValidationAttributes
{
    public class CourseTitleMustBeDifferentFromDescriptionAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var course = (CourseManipulationDto)validationContext.ObjectInstance;
            if(course.Title == course.Description)
            {
                return new ValidationResult(
                    "The provided description should be different from the title.",
                    new[] {nameof(CourseManipulationDto) }
                    );
            }

            return ValidationResult.Success;
        }
    }
}
