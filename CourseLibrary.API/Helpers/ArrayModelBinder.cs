using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace CourseLibrary.API.Helpers
{
    public class ArrayModelBinder : IModelBinder
    {
        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            // This binder works only on IEnumerable types
            if (!bindingContext.ModelMetadata.IsEnumerableType)
            {
                bindingContext.Result = ModelBindingResult.Failed();
                return Task.CompletedTask;
            }

            // Getting the inputted value through the value provider
            var value = bindingContext.ValueProvider.
                GetValue(bindingContext.ModelName).ToString();

            if (string.IsNullOrWhiteSpace(value))
            {
                bindingContext.Result = ModelBindingResult.Success(null);
                return Task.CompletedTask;
            }

            // The value isn't null or white space,
            // and the type is IEnumerable
            // Get the enumearble's type, and a converter

            var elementType = bindingContext.ModelType.GetTypeInfo().GenericTypeArguments[0];
            var converter = TypeDescriptor.GetConverter(elementType);

            // Converting each item in the value list to the enumerable type
            var values = value.Split(new[] { ","}, StringSplitOptions.RemoveEmptyEntries)
                .Select(x=> converter.ConvertFromString(x.Trim()))
                .ToArray();

            // Creating an array of that type , and set it as the model value
            var typedValues = Array.CreateInstance(elementType,values.Length);
            values.CopyTo(typedValues, 0);
            bindingContext.Model = typedValues;

            // Return  a successfull result, passing in the Model
            bindingContext.Result = ModelBindingResult.Success(bindingContext.Model);
            return Task.CompletedTask;


                

        }
    }
}
