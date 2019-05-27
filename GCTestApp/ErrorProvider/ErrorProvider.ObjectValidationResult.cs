namespace GCTestApp.ErrorProvider
{
    partial class ErrorProvider
    {
        public sealed class ObjectValidationResult
        {
        	public string CollectionPath;
        	public object CollectionItem;
            public string PropertyName;
            public string ErrorMessage;
            public ValidationErrorType ErrorType;
        }
    }
}
