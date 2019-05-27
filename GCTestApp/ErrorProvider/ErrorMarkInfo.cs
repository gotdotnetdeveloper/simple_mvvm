using System;
using System.Collections.Generic;

namespace GCTestApp.ErrorProvider
{

    /// <summary>
    /// Текст ошибки и его тип (Предупреждение/Ошибка).
    /// </summary>
	public class ErrorMarkInfo
	{
		private string _errorMessage;

		public ValidationErrorType ErrorType { get; set; }

		public string ErrorMessage
		{
			get
			{
				if (_errorMessage == null)
				{
					_errorMessage = ErrorMessages.Count == 0 ? string.Empty : String.Join(Environment.NewLine, ErrorMessages);
				}
				return _errorMessage == string.Empty ? null : _errorMessage;
			}
		}

		internal readonly HashSet<string> ErrorMessages;

		public ErrorMarkInfo(ValidationErrorType errorType, string errorMessage)
		{
			ErrorType = errorType;
			ErrorMessages = new HashSet<string>(new[]{errorMessage});
		}

		public ErrorMarkInfo(ValidationErrorType errorType, IEnumerable<string> errorMessages)
		{
			ErrorType = errorType;
			ErrorMessages = new HashSet<string>(errorMessages);
		}

		public void AddErrorMessage(string errorMessage)
		{
			ErrorMessages.Add(errorMessage);
			_errorMessage = null;
		}

		public void ClearErrorMessage()
		{
			ErrorMessages.Clear();
			_errorMessage = null;
		}

		public void ReplaceErrorMessage(string errorMessage)
		{
			ClearErrorMessage();
			AddErrorMessage(errorMessage);
		}
	}
}
