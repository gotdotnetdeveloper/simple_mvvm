using System;
using System.Collections.Generic;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace GCTestApp.ErrorProvider
{
    partial class ErrorProvider
    {
        public static readonly DependencyProperty ValidationForWholeCollectionIsVisibleProperty =
            DependencyProperty.RegisterAttached("ValidationForWholeCollectionIsVisible", typeof(bool), typeof(ErrorProvider),
                new FrameworkPropertyMetadata(true) { Inherits = true });

        public static void SetValidationForWholeCollectionIsVisible(DependencyObject element, bool value)
        {
            element.SetValue(ValidationForWholeCollectionIsVisibleProperty, value);
        }

        public static bool GetValidationForWholeCollectionIsVisible(DependencyObject element)
        {
            return (bool)element.GetValue(ValidationForWholeCollectionIsVisibleProperty);
        }

        private sealed class ControlInfo
        {
            private static readonly Func<DependencyObject, Label> GetLabeledByFunc;

            private string _errorMessage;
            private List<string> _bindingPathes;

            internal readonly HashSet<string> ErrorMessages = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            static ControlInfo()
            {
                var methodInfo = typeof(Label).GetMethod("GetLabeledBy", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.InvokeMethod);
                GetLabeledByFunc = (Func<DependencyObject, Label>)Delegate.CreateDelegate(typeof(Func<DependencyObject, Label>), methodInfo);
            }

            public ControlInfo(FrameworkElement element, ErrorProvider errorProvider, string bindingPath)
            {
                Element = element;
                AggregateSubErrors = GetAggregateSubErrors(element);
                ErrorProvider = errorProvider;
                _bindingPathes = new List<string> { bindingPath };

	            element.Loaded += OnElementLoaded;
                //element.IsVisibleChanged += OnIsVisibleChanged;
				OnElementLoaded(element, new RoutedEventArgs(LoadedEvent));
            }

            public void AddBindingPath(string path)
            {
                if (_bindingPathes == null)
                    _bindingPathes = new List<string>();

                _bindingPathes.Add(path);
            }

            /// <summary>
            /// Controlled Element
            /// </summary>
            public FrameworkElement Element
            {
                get; }

            /// <summary>
            /// Parent ErrorProvider
            /// </summary>
            private ErrorProvider ErrorProvider
            {
                get; }

            public List<string> BindingPathes
            {
                get { return _bindingPathes; }
            }

            private bool AggregateSubErrors { get; }

            public bool IsDataRequired
            {
                private get;
                set;
            }

            public ValidationErrorType ErrorType
            {
                get;
                set;
            }

            public string ErrorMessage
            {
                get
                {
                    if (_errorMessage == null)
                    {
                        _errorMessage = ErrorMessages.Count == 0 ? string.Empty : string.Join(Environment.NewLine, ErrorMessages);
                    }
                    return _errorMessage == string.Empty ? null : _errorMessage;
                }
            }

            /// <summary>
            /// Значок ошибки валидации
            /// </summary>
            private IErrorControl ErrorControl
            {
                get;
                set;
            }

            private bool IsErrorControlInPlace { get; set; }

            public CollectionErrorProvider CollectionErrorProvider { get; set; }

            public SubtreeErrorProvider SubtreeErrorProvider { get; set; }

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


            private Label GetLabeledBy()
            {
                return GetLabeledByFunc(Element);
            }

            /// <summary>
            /// Обновление звездочки в метках обязательных для заполнения полей
            /// </summary>
            public void RefreshRequiredMark()
            {
                var label = GetLabeledBy();
                if (label == null)
                    return;

                label.SetIsTargetRequiredInternal(IsDataRequired);
            }

            /// <summary>
            /// Обновление значка ошибки валидации
            /// </summary>
            public void RefreshValidationMark()
            {
                if (CollectionErrorProvider != null)
                {
                    CollectionErrorProvider.RefreshValidationMark();
                    //return;
                }
                if (ErrorType == ValidationErrorType.None && ErrorControl == null)
                    return;     // Error control-а нет и не нужен

                if (ErrorType == ValidationErrorType.None || ErrorControl == null || ((FrameworkElement)ErrorControl).Parent == null)
                {
                    // Надо удалить или создать ErrorControl
                    var associatedErrorControl = GetErrorControl(Element);
                    // var grid = associatedErrorControl == null ? Element.Parent as Grid : null;
                    if (ErrorProvider._isViewUnloaded /*|| (grid == null && associatedErrorControl == null)*/)
                        return;
                    if (ErrorType == ValidationErrorType.None)
                    {
                        if (associatedErrorControl == null)
                        {
                            // grid.Children.Remove((UIElement)ErrorControl);
                            RemoveErrorControl();
                            // ErrorControl = null;
                        }
                        else
                        {
                            associatedErrorControl.Visibility = Visibility.Collapsed;
                        }
                        return;
                    }
                    if (associatedErrorControl == null)
                    {
                        //var elementColumnSpan = Grid.GetColumnSpan(Element);
                        //var columnSpan = elementColumnSpan > 1 ? elementColumnSpan : 0;
                        //var column = Grid.GetColumn(Element); // + columnSpan;
                        ErrorControl = CreateErrorControl();
                    }
                    else
                    {
                        ErrorControl = associatedErrorControl;
                        IsErrorControlInPlace = true;
                    }
                }

                if (ErrorControl != null)
                {
                    ErrorControl.ErrorType = ErrorType;
                    ErrorControl.ToolTip = ErrorMessage;
                    if (IsErrorControlInPlace)
                    {
                        ((UIElement)ErrorControl).Visibility = (ErrorType == ValidationErrorType.None) ? Visibility.Collapsed : Visibility.Visible;
                    }

                    var isCollection = CollectionErrorProvider != null;
                    var validationForWholeCollectionIsVisible = GetValidationForWholeCollectionIsVisible(Element);
                    var hideValidationForWholeCollection = isCollection && !validationForWholeCollectionIsVisible;
                    if (hideValidationForWholeCollection)
                        ((UIElement)ErrorControl).Visibility = Visibility.Collapsed;
                }
            }

            private void OnElementLoaded(object sender, RoutedEventArgs e)
            {
                var adorner = ErrorControl as Adorner;
                if (adorner == null)
                    return;
                var layerForErrorControl = adorner.Parent as AdornerLayer;
                var layerForElement = AdornerLayer.GetAdornerLayer(Element);
                if (layerForElement == null)
                    return;
                if (ReferenceEquals(layerForErrorControl, layerForElement))
                    return;
                if (layerForErrorControl != null)
                {
                    layerForErrorControl.Remove(adorner);
                }
                layerForElement.Add(adorner);
            }

            //private void OnIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
            //{
            //    ErrorProvider.QueueValidate();
            //}

            private void RemoveErrorControl()
            {
                if (ErrorControl == null)
                    return;
                var adorner = ErrorControl as Adorner;
                if (adorner != null)
                {
                    try
                    {
                        var layer = adorner.Parent as AdornerLayer ?? AdornerLayer.GetAdornerLayer(Element);
                        if (layer != null)
                        {
                            layer.Remove((Adorner)ErrorControl);
                        }
                    }
                    // ReSharper disable once EmptyGeneralCatchClause
                    catch
                    {
                    }
                }
                ErrorControl = null;
            }

            private IErrorControl CreateErrorControl(/*Grid grid, int row, int column, int columnSpan*/)
            {
                var layer = AdornerLayer.GetAdornerLayer(Element);
                var retVal = new ErrorAdorner(Element);
                if (layer != null)
                    layer.Add(retVal);
                return retVal;
                /*
				switch (ErrorProvider.ErrorDisplay)
				{
					case ErrorDisplay.Border:
						var brd = new ErrorBorder();
						grid.Children.Add(brd);
						Grid.SetColumn(brd, column);
						Grid.SetRow(brd, row);
						if (columnSpan > 0)
							Grid.SetColumnSpan(brd, columnSpan);
						brd.BindToElement(Element);
						return brd;
					default:
						var ec = new ErrorControl {
							VerticalAlignment = VerticalAlignment.Top
						};
						grid.Children.Add(ec);
						Grid.SetColumn(ec, column + (columnSpan == 0 ? 0 : columnSpan - 1));
						Grid.SetRow(ec, row);
						return ec;
				}
        		return null;
				 */
            }

            public void UnBind()
            {
                if (ErrorControl != null)
                {
                    ErrorType = ValidationErrorType.None;
                    RefreshValidationMark();
                }
				if (CollectionErrorProvider != null)
				{
					CollectionErrorProvider.UnInitialize();
				}
	            if (SubtreeErrorProvider != null)
	            {
					SubtreeErrorProvider.UnInitialize();
				}
				if (Element != null)
				{
				    Element.Loaded -= OnElementLoaded;
				    //Element.IsVisibleChanged -= OnIsVisibleChanged;
				}
            }

            public void ClearTemporaryErrorInfo()
            {
                ErrorType = ValidationErrorType.None;
                ClearErrorMessage();
                if (CollectionErrorProvider != null)
                    CollectionErrorProvider.ClearErrors();
            }

            public void ProvideCollectionError(ObjectValidationResult objectValidationResult)
            {
                if (CollectionErrorProvider != null)
                    CollectionErrorProvider.AddObjectError(objectValidationResult);
            }

            public void BuildCollectionErrors()
            {
                if (CollectionErrorProvider != null)
                {
                    var mark = CollectionErrorProvider.GetMostErrorMark();
                    if (mark != null)
                    {
                        ErrorType = mark.ErrorType;
                        ErrorMessages.Clear();

                        if(mark.ErrorMessages != null)
                        foreach (var markErrorMessage in mark.ErrorMessages)
                            ErrorMessages.Add(markErrorMessage);
                        
                        _errorMessage = null;
                    }
                }
            }

            public bool CanShowErrorForProperty(string propertyName)
            {
                // ReSharper disable once ForCanBeConvertedToForeach
                for (var i = 0; i < BindingPathes.Count; i++)
                {
                    var bp = BindingPathes[i];
                    if (AggregateSubErrors)
                    {
                        if (propertyName.StartsWith(bp, StringComparison.Ordinal) && (propertyName.Length == bp.Length || propertyName[bp.Length] == '.'))
                            return true;
                    }
                    else
                    {
                        if (bp == propertyName)
                            return true;
                    }
                }
                return false;
            }
        }
    }
}
