namespace GCTestApp.ErrorProvider
{
	public interface IErrorControl
	{
		ValidationErrorType ErrorType { get; set; }
		object ToolTip { get; set; }
	}
}