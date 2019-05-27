using System;
using System.Windows;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Media;
using JetBrains.Annotations;

namespace GCTestApp.ErrorProvider
{
	public class ErrorAdorner : Adorner, IErrorControl
	{
		private readonly ErrorControl _child;

		public static readonly DependencyProperty ChildMarginProperty = DependencyProperty.Register(
			"ChildMargin", typeof(Thickness), typeof(ErrorAdorner), new PropertyMetadata(default(Thickness)));

		public Thickness ChildMargin
		{
			get { return (Thickness)GetValue(ChildMarginProperty); }
			set { SetValue(ChildMarginProperty, value); }
		}

		public ErrorAdorner([NotNull] UIElement adornedElement) : base(adornedElement)
		{
			_child = new ErrorControl
			{
				VerticalAlignment = VerticalAlignment.Top
			};
			if (adornedElement is FrameworkElement) { 
				SetBinding(ChildMarginProperty, new Binding {Source = adornedElement, Path = new PropertyPath(MarginProperty)});}
			AddVisualChild(_child);
		}

		protected override int VisualChildrenCount
		{
			get
			{
				return 1;
			}
		}

		protected override Visual GetVisualChild(int index)
		{
			if (index != 0) throw new ArgumentOutOfRangeException();
			return _child;
		}

		public override GeneralTransform GetDesiredTransform(GeneralTransform transform)
		{
			var retVal = base.GetDesiredTransform(transform);
			var childMargin = ChildMargin;
			if (childMargin.Equals(default(Thickness)))
				return retVal;
			if (retVal == null)
				return new TranslateTransform(0, -childMargin.Top);
			var trtr = retVal as TranslateTransform;
			if (trtr != null)
			{
				return new TranslateTransform(trtr.X, trtr.Y - childMargin.Top);
			}
			var tr = new GeneralTransformGroup();
			tr.Children.Add(new TranslateTransform(0, -childMargin.Top));
			tr.Children.Add(retVal);
			return tr;
		}

		protected override Size MeasureOverride(Size constraint)
		{
			var result = base.MeasureOverride(constraint);
			return new Size(result.Width + ChildMargin.Right, result.Height + ChildMargin.Top + ChildMargin.Bottom);
		}

		protected override Size ArrangeOverride(Size finalSize)
		{
			_child.Arrange(new Rect(new Point(0, 0), finalSize));
			return base.ArrangeOverride(finalSize);
		}

		public ValidationErrorType ErrorType
		{
			get { return _child.ErrorType; } 
			set { _child.ErrorType = value; }
		}

		object IErrorControl.ToolTip
		{
			get { return _child.ToolTip; }
			set { _child.ToolTip = value; }
		}
	}
}