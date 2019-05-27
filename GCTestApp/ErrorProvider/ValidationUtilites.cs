using System;
using System.Collections.Generic;
using System.Linq;
using FluentValidation;
using FluentValidation.Internal;
using FluentValidation.Results;

namespace GCTestApp.ErrorProvider
{
	internal static class ValidationUtilites
	{
        public static readonly DefaultValidatorSelector Instance = new DefaultValidatorSelector();

  //      internal static void AddValidationResults(string objectPath, ref List<ErrorProvider.ObjectValidationResult> errorList, ValidationFailure validationResult)
		//{
		//	foreach (var s in validationResult.MemberNames)
		//	{
		//		if (errorList == null)
		//			errorList = new List<ErrorProvider.ObjectValidationResult>();
		//		errorList.Add(new ErrorProvider.ObjectValidationResult {
		//			PropertyName = BuildPropertyPath(objectPath, s),
		//			ErrorMessage = validationResult.ErrorMessage,
		//			ErrorType = validationResult.ErrorType
		//		});
		//	}
		//}

		private static IEnumerable<ValidationResult> FluentValidationResultsToValidationResultsEx(
			IEnumerable<ValidationResult> fluentValidationResults)
        {
            return null; // fluentValidationResults.SelectMany(r => r.Errors);
        }


		/// <summary>
		/// Добавление ошибок валидации в коллекцию
		/// </summary>
		/// <param name="objectPath"></param>
		/// <param name="errorList"></param>
		/// <param name="errors"></param>
		internal static void AddValidationResults(string objectPath, ref List<ErrorProvider.ObjectValidationResult> errorList, IEnumerable<ValidationResult> errors)
		{
			//if (errors == null)
			//	return;
			//foreach (var e in errors)
			//{
			//	AddValidationResults(objectPath, ref errorList, e);
			//}
		}

		internal static IEnumerable<ValidationResult> ValidateViaValidator(
			object value, IValidator validator, 
			HashSet<string> requiredProperties, IServiceProvider serviceProvider)
		{
			//var validatorResults = new List<ValidationResult>();
			//if (validator != null)
			//{
			//	var ctx = new ValidationContext(value, new PropertyChain(), Instance );
			//	if (requiredProperties != null)
			//		ctx.RequiredPropertiesSet = requiredProperties;
			//	var fluentResult = validator.Validate(ctx);
			//	validatorResults.Add(fluentResult);
			//}

            return null;
            // ReSharper disable LoopCanBeConvertedToQuery
            //return FluentValidationResultsToValidationResultsEx(validatorResults).ToList();
        }

		internal static string BuildPropertyPath(string objectPath, string propertyName)
		{
			if (propertyName.IsNullOrEmpty())
				return objectPath;
			if (objectPath.IsNullOrEmpty())
				return propertyName;
			return objectPath + '.' + propertyName;
		}
	}
}