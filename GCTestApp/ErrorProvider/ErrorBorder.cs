using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace GCTestApp.ErrorProvider
{
    /// <summary>
    /// Красная рамка, для обозначения ошибки.
    /// </summary>
	public class ErrorBorder : Border, IErrorControl
	{
		public ErrorBorder()
		{
			BorderThickness = new Thickness(2);
			BorderBrush = Brushes.Red;
			Width = 100;
			Height = 50;
            SetValue(ToolTipService.ShowDurationProperty, 60000);
		}

		#region Implementation of IErrorControl

		public ValidationErrorType ErrorType { get; set; }

		#endregion

		public void BindToElement(FrameworkElement element)
		{
			SetBinding(WidthProperty, new Binding("ActualWidth") {Source = element});
			SetBinding(HeightProperty, new Binding("ActualHeight") { Source = element });
			SetBinding(HorizontalAlignmentProperty, new Binding("HorizontalAlignment") { Source = element });
			SetBinding(VerticalAlignmentProperty, new Binding("VerticalAlignment") { Source = element });
		}
	}
}
