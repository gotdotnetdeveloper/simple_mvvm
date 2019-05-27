using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace GCTestApp.ErrorProvider
{
	/// <summary>
	/// Контрол показывает восклицательный знак.   
	/// </summary>
	public class ErrorControl : ContentControl, IErrorControl
	{
		/// <summary>
		/// Уровень ошибки
		/// </summary>
		public static readonly DependencyProperty ErrorTypeProperty =
			DependencyProperty.Register("ErrorType", typeof(ValidationErrorType), typeof(ErrorControl),
				new UIPropertyMetadata(ValidationErrorType.None, OnErrorTypePropertyChanged));

		public static readonly DependencyProperty ErrorMarkInfoProperty =
			DependencyProperty.Register("ErrorMarkInfo", typeof(ErrorMarkInfo),
				typeof(ErrorControl), new UIPropertyMetadata(null, OnErrorMarkInfoPropertyChanged));

		public static readonly DependencyProperty CollapsedIfNoErrorProperty =
			DependencyProperty.Register("CollapsedIfNoError", typeof(bool), typeof(ErrorControl), new PropertyMetadata(default(bool), OnCollapsedIfNoErrorPropertyChanged));

        public static readonly DependencyProperty ErrorImageSourceProperty =
            DependencyProperty.RegisterAttached("ErrorImageSource", typeof(ImageSource), typeof(ErrorControl),
                new FrameworkPropertyMetadata(default(ImageSource), OnAttachedImageSourcePropertyChanged) { Inherits = true});

        public static readonly DependencyProperty WarningImageSourceProperty =
            DependencyProperty.RegisterAttached("WarningImageSource", typeof(ImageSource), typeof(ErrorControl),
                new FrameworkPropertyMetadata(default(ImageSource), OnAttachedImageSourcePropertyChanged) { Inherits = true });

        public static readonly DependencyProperty ServerImageSourceProperty =
            DependencyProperty.RegisterAttached("ServerImageSource", typeof(ImageSource), typeof(ErrorControl),
                new FrameworkPropertyMetadata(default(ImageSource), OnAttachedImageSourcePropertyChanged) { Inherits = true });

        private readonly Image _image;

		private Image Image {get { return _image; }}

		private static void OnErrorMarkInfoPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			var c = d as ErrorControl;
			if (c != null)
			{
				if (c.ErrorMarkInfo == null)
				{
					c.ErrorType = ValidationErrorType.None;
					c.ToolTip = null;
				}
				else
				{
					c.ErrorType = c.ErrorMarkInfo.ErrorType;
					c.ToolTip = c.ErrorMarkInfo.ErrorMessage;
				}
			}
		}

		public ErrorMarkInfo ErrorMarkInfo
		{
			get { return (ErrorMarkInfo)GetValue(ErrorMarkInfoProperty); }
			set { SetValue(ErrorMarkInfoProperty, value); }
		}

		public bool CollapsedIfNoError
		{
			get { return (bool)GetValue(CollapsedIfNoErrorProperty); }
			set { SetValue(CollapsedIfNoErrorProperty, value); }
		}

        public static void SetErrorImageSource(DependencyObject element, ImageSource value)
        {
            element.SetValue(ErrorImageSourceProperty, value);
        }

        public static ImageSource GetErrorImageSource(DependencyObject element)
        {
            return (ImageSource)element.GetValue(ErrorImageSourceProperty);
        }

        public static void SetWarningImageSource(DependencyObject element, ImageSource value)
        {
            element.SetValue(WarningImageSourceProperty, value);
        }

        public static ImageSource GetWarningImageSource(DependencyObject element)
        {
            return (ImageSource)element.GetValue(WarningImageSourceProperty);
        }

        public static void SetServerImageSource(DependencyObject element, ImageSource value)
        {
            element.SetValue(ServerImageSourceProperty, value);
        }

        public static ImageSource GetServerImageSource(DependencyObject element)
        {
            return (ImageSource)element.GetValue(ServerImageSourceProperty);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ErrorControl"/> class.
        /// </summary>
        public ErrorControl()
		{
			_image = new Image();
			Content = _image;
			UpdateImage();
			SetValue(ToolTipService.ShowDurationProperty, 60000);
		}

		/// <summary>
		/// Уровень ошибки
		/// </summary>
		public ValidationErrorType ErrorType
		{
			get { return (ValidationErrorType)GetValue(ErrorTypeProperty); }
			set
			{
				SetValue(ErrorTypeProperty, value);
			}
		}

		private static void OnErrorTypePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			var c = d as ErrorControl;
			if (c != null)
				c.UpdateImage();
		}

		private static void OnCollapsedIfNoErrorPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			var c = d as ErrorControl;
			if (c != null)
				c.UpdateImage();
		}

        private static void OnAttachedImageSourcePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var c = d as ErrorControl;
            if (c != null)
                c.UpdateImage();
        }

        private void UpdateImage()
		{
			if (ErrorType == ValidationErrorType.None)
			{
				Image.Visibility = Visibility.Hidden;
				Image.Source = null;
				if (CollapsedIfNoError)
					Visibility = Visibility.Collapsed;
				return;
			}
			if (CollapsedIfNoError)
				Visibility = Visibility.Visible;

			ImageSource imageSource = null;
			switch (ErrorType)
			{
				case ValidationErrorType.Error:
			        imageSource = TakeImageSourceOrLoadByKeyIfDefault(GetErrorImageSource(this), ImageResourceKeys.SmallRedAttention);
                    break;
				case ValidationErrorType.Warning:
                    imageSource = TakeImageSourceOrLoadByKeyIfDefault(GetWarningImageSource(this), ImageResourceKeys.SmallNavyAttention);
                    break;
				case ValidationErrorType.Server:
                    imageSource = TakeImageSourceOrLoadByKeyIfDefault(GetServerImageSource(this), ImageResourceKeys.SmallYellowAttention);
                    break;
			}
			if (imageSource != null)
				Image.Source = imageSource;

			Image.Visibility = Visibility.Visible;
		}

	    private static ImageSource TakeImageSourceOrLoadByKeyIfDefault(ImageSource imageSource, string defaultImageKey)
	    {
	        if (imageSource != null)
                return imageSource;
            if (defaultImageKey != null)
                return Application.Current.FindResource(defaultImageKey) as ImageSource;
	        return default(ImageSource);
	    }
	}
}