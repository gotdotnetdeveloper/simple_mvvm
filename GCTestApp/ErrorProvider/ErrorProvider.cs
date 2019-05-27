using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Threading;
using JetBrains.Annotations;

namespace GCTestApp.ErrorProvider
{
	/// <summary>
	/// Обеспечивает UI валидации
	/// </summary>
	public partial class ErrorProvider : AdornerDecorator
	{
		#region Fields

		/// <summary>
		/// The <see cref="HasError"/> dependency property.
		/// </summary>
		public static readonly DependencyProperty HasErrorProperty = DependencyProperty.Register("HasError", typeof(bool), typeof(ErrorProvider), new UIPropertyMetadata(false, OnHasErrorChanged));

		/// <summary>
		/// The IsValidate attached property.
		/// </summary>
		public static readonly DependencyProperty IsValidateProperty = DependencyProperty.RegisterAttached("IsValidate", typeof(bool?), typeof(ErrorProvider), new UIPropertyMetadata(null, OnIsValidateChanged));

		/// <summary>
		/// The IsValidate attached property.
		/// </summary>
		public static readonly DependencyProperty IsValidateNonVisualProperty = DependencyProperty.RegisterAttached("IsValidateNonVisual", typeof(bool), typeof(ErrorProvider), new UIPropertyMetadata(false, IsValidateNonVisualChanged));

		/// <summary>
		/// Что ErrorProvider будет валидировать
		/// </summary>
		public static readonly DependencyProperty ValidateProperty = DependencyProperty.RegisterAttached("Validate", typeof(object), typeof(ErrorProvider), new UIPropertyMetadata(null));

		/// <summary>
        /// Агрегировать ошибки валидации поддерева свойств объекта
        /// </summary>
        public static readonly DependencyProperty AggregateSubErrorsProperty = DependencyProperty.RegisterAttached("AggregateSubErrors", typeof(bool), typeof(ErrorProvider), new UIPropertyMetadata(false));

        /// <summary>
        /// Слушать изменения свойств объекта и валидировать его
        /// </summary>
        public static readonly DependencyProperty ValidateObjectProperty = DependencyProperty.RegisterAttached("ValidateObject", typeof(bool), typeof(ErrorProvider), new UIPropertyMetadata(false));

        /// <summary>
		/// ErrorProvider, привязанный к контролу будет валидировать
		/// </summary>
		private static readonly DependencyProperty ErrorProviderProperty = DependencyProperty.RegisterAttached("ErrorProvider", typeof(ErrorProvider), typeof(ErrorProvider), new UIPropertyMetadata(null));

		/// <summary>
		/// Для байндинга к Subtree контрола
		/// </summary>
		private static readonly DependencyProperty SubtreeProperty = DependencyProperty.RegisterAttached("Subtree", typeof(object), typeof(ErrorProvider), new UIPropertyMetadata(null, OnSubtreeChanged));

		/// <summary>
		/// The <see cref="ErrorMessage"/> dependency property.
		/// </summary>
		public static readonly DependencyProperty ErrorMessageProperty = DependencyProperty.Register("ErrorMessage", typeof(string), typeof(ErrorProvider), new UIPropertyMetadata(null, OnErrorMessageChanged));

		/// <summary>
		/// ErrorProvider для валидации коллекций.
		/// </summary>
		public static readonly DependencyProperty CollectionErrorProviderProperty =
			DependencyProperty.RegisterAttached("CollectionErrorProvider", typeof(CollectionErrorProvider), typeof(ErrorProvider), new UIPropertyMetadata(null));

		/// <summary>
		/// ErrorProvider для валидации поддеревьев.
		/// </summary>
		public static readonly DependencyProperty SubtreeErrorProviderProperty =
			DependencyProperty.RegisterAttached("SubtreeErrorProvider", typeof(SubtreeErrorProvider), typeof(ErrorProvider), new UIPropertyMetadata(null));

		/// <summary>
		/// The <see cref="ErrorDisplay"/> dependency property.
		/// </summary>
		public static readonly DependencyProperty ErrorDisplayProperty = DependencyProperty.Register("ErrorDisplay", typeof(ErrorDisplay), typeof(ErrorProvider), new UIPropertyMetadata(ErrorDisplay.Glyph));

		/// <summary>
        /// Набор биндингов к объектам, PropertyChanges которых будут слушаться, а сами объекты - валидироваться
        /// </summary>
        public static readonly DependencyProperty ValidateBindingsProperty = DependencyProperty.Register("ValidateBindings", typeof(List<ErrorProviderBinding>), typeof(ErrorProvider), new UIPropertyMetadata(null));

        /// <summary>
		/// <see cref="FrameworkElement"/> types with dependency properties.
		/// </summary>
		private static readonly Dictionary<Type, DependencyProperty[]> ElementTypesWithDependencyProperties = new Dictionary<Type, DependencyProperty[]>();

		/// <summary>
		/// The list of forced validation elements. Здесь регистрируются контролы из template.
		/// </summary>
		private readonly HashSet<DependencyObject> _forcedValidationElements;

		/// <summary>
		/// The List of elements, controlled by this ErrorProvider
		/// </summary>
		private readonly Dictionary<FrameworkElement, ControlInfo> _controlledElements;

		/// <summary>
		/// The List of Complex Data Objects
		/// </summary>
		private readonly SortedDictionary<string, DataObjectInfo> _dataObjects;

		private bool _justHasErrors;

		private IView _view;
		private bool _isViewUnloaded;
		private string _justErrorMessage;
		private bool _isProcessReinitializePending;

		#endregion

		#region .ctor
		/// <summary>
		/// Initializes a new instance of the <see cref="ErrorProvider"/> class. 
		/// </summary>
		public ErrorProvider()
		{
			if (!DesignerHelper.IsInDesignModeStatic)
			{
				Loaded += OnErrorProviderLoaded;
				DataContextChanged += OnErrorProviderDataContextChanged;
			}
			_controlledElements = new Dictionary<FrameworkElement, ControlInfo>();
			_dataObjects = new SortedDictionary<string, DataObjectInfo>();
			_forcedValidationElements = new HashSet<DependencyObject>();
            ValidateBindings = new List<ErrorProviderBinding>();
		}

		#endregion

		#region Properties

		/// <summary>
		/// Признак наличия ошибок в связанных элементах.
		/// </summary>
		public bool HasError
		{
			get { return (bool)GetValue(HasErrorProperty); }
			set { SetValue(HasErrorProperty, value); }
		}

		private static void OnHasErrorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			CommandManager.InvalidateRequerySuggested();
		}

		/// <summary>
		/// Текст первой ошибки.
		/// </summary>
		public string ErrorMessage
		{
			get { return (string)GetValue(ErrorMessageProperty); }
			set { SetValue(ErrorMessageProperty, value); }
		}

		private static void OnErrorMessageChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			CommandManager.InvalidateRequerySuggested();
		}

		/// <summary>
	    /// Набор биндингов к объектам, PropertyChanges которых будут слушаться, а сами объекты - валидироваться
	    /// </summary>
	    public List<ErrorProviderBinding> ValidateBindings
	    {
            get { return (List<ErrorProviderBinding>)GetValue(ValidateBindingsProperty); }
            set { SetValue(ValidateBindingsProperty, value); }
        }

        /// <summary>
		/// Текст первой ошибки.
		/// </summary>
		public ErrorDisplay ErrorDisplay
		{
			get { return (ErrorDisplay)GetValue(ErrorDisplayProperty); }
			set { SetValue(ErrorDisplayProperty, value); }
		}

		public bool IsValidateNonVisual
		{
			get { return (bool)GetValue(IsValidateNonVisualProperty); }
			set { SetValue(IsValidateNonVisualProperty, value); }
		}

		public static bool? GetIsValidateNonVisual(DependencyObject obj)
		{
			return (bool?)obj.GetValue(IsValidateNonVisualProperty);
		}

		public static void SetIsValidateNonVisual(DependencyObject obj, bool? value)
		{
			obj.SetValue(IsValidateNonVisualProperty, value);
		}

		/// <summary>
		/// Get attached property <see cref="IsValidateProperty"/>.
		/// </summary>
		/// <param name="obj">
		/// The Dependency Object.
		/// </param>
		/// <returns>
		/// The value of property.
		/// </returns>
		public static bool? GetIsValidate(DependencyObject obj)
		{
			return (bool?)obj.GetValue(IsValidateProperty);
		}

		/// <summary>
		/// Set attached property <see cref="IsValidateProperty"/>.
		/// </summary>
		/// <param name="obj">
		/// The Dependency Object.
		/// </param>
		/// <param name="value">
		/// The value.
		/// </param>
		public static void SetIsValidate(DependencyObject obj, bool? value)
		{
			obj.SetValue(IsValidateProperty, value);
		}

		/// <summary>
        /// Что ErrorProvider будет валидировать
		/// </summary>
		/// <param name="obj"></param>
		/// <param name="value"></param>
		public static void SetValidate(DependencyObject obj, object value)
		{
			obj.SetValue(ValidateProperty, value);
		}

		/// <summary>
        /// Что ErrorProvider будет валидировать
		/// </summary>
		/// <param name="obj"></param>
		public static object GetValidate(DependencyObject obj)
		{
			return obj.GetValue(ValidateProperty);
		}

        /// <summary>
        /// Агрегировать ошибки валидации поддерева свойств объекта
        /// </summary>
	    public static void SetAggregateSubErrors(DependencyObject obj, bool value)
	    {
	        obj.SetValue(AggregateSubErrorsProperty, value);
	    }

        /// <summary>
        /// Агрегировать ошибки валидации поддерева свойств объекта
        /// </summary>
        public static bool GetAggregateSubErrors(DependencyObject obj)
        {
            return (bool)obj.GetValue(AggregateSubErrorsProperty);
        }

        /// <summary>
        /// Слушать изменения свойств объекта и валидировать его
        /// </summary>
	    public static void SetValidateObject(DependencyObject obj, bool value)
        {
            obj.SetValue(ValidateObjectProperty, value);
        }

        /// <summary>
        /// Слушать изменения свойств объекта и валидировать его
        /// </summary>
        public static bool GetValidateObject(DependencyObject obj)
        {
            return (bool)obj.GetValue(ValidateObjectProperty);
        }

		private static void IsValidateNonVisualChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			if (DesignerHelper.IsInDesignModeStatic)
				return;
			if (e.NewValue != null)
			{
				var errorProvider = d as ErrorProvider;
                errorProvider = errorProvider ?? d.FindParent<ErrorProvider>();
				if (errorProvider != null)
				{
                    errorProvider.IsValidateNonVisual = (bool)e.NewValue;
				}
			}
		}

		private static void OnIsValidateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			if (DesignerHelper.IsInDesignModeStatic)
				return;
			if (e.NewValue != null)
			{
				var errorProvider = d.FindParent<ErrorProvider>();
				if (errorProvider != null)
				{
					SetErrorProvider(d, errorProvider);
					if (((bool?)e.NewValue).GetValueOrDefault())
					{
						errorProvider._forcedValidationElements.Add(d);
					}
					else
					{
						errorProvider._forcedValidationElements.Remove(d);
					}
					errorProvider.CallProcessReinitialize();
				}
				else
				{
					var c = d as UIElement;
					if (c != null)
					{
						c.IsVisibleChanged -= OnTrackControlAppearance;
						c.IsVisibleChanged += OnTrackControlAppearance;
					}
				}
			}
		}

		public static object GetSubtree(DependencyObject obj)
		{
			return obj.GetValue(SubtreeProperty);
		}

		public static void SetSubtree(DependencyObject obj, object value)
		{
			obj.SetValue(SubtreeProperty, value);
		}

		private static void OnSubtreeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			if (d is UIElement)
			{
				var errorProvider = GetErrorProvider(d);
				if (errorProvider != null)
				{
					/*
					if (isVisible)
					{
						if (!errorProvider._forcedValidationElements.Contains(c))
							errorProvider._forcedValidationElements.Add(c);
					}
					 */
					errorProvider.CallProcessReinitialize();
					// errorProvider.Initialize();
				}
			}
		}

		private static ErrorProvider GetErrorProvider(DependencyObject obj)
		{
			return (ErrorProvider)obj.GetValue(ErrorProviderProperty);
		}

		private static void SetErrorProvider(DependencyObject obj, ErrorProvider value)
		{
			obj.SetValue(ErrorProviderProperty, value);
		}

		private static void OnTrackControlAppearance(object sender, DependencyPropertyChangedEventArgs e)
		{
			var c = (UIElement)sender;
			var isVisible = (bool)(e.NewValue);
			var errorProvider = c.FindParent<ErrorProvider>();
			if (errorProvider != null)
			{
				SetErrorProvider(c, errorProvider);
				if (isVisible)
				{
					errorProvider._forcedValidationElements.Add(c);
				}
				errorProvider.CallProcessReinitialize();
				// errorProvider.Initialize();
			}
		}

		private void CallProcessReinitialize()
		{
			if (_isProcessReinitializePending)
				return;
			Dispatcher.BeginInvoke(DispatcherPriority.Background, (Action)(() =>
			{
				_isProcessReinitializePending = false;
				if (IsLoaded && !_isViewUnloaded)
				{
					Initialize();
				}
			}));
		}

		public static CollectionErrorProvider GetCollectionErrorProvider(DependencyObject obj)
		{
			return (CollectionErrorProvider)obj.GetValue(CollectionErrorProviderProperty);
		}

		public static void SetCollectionErrorProvider(DependencyObject obj, CollectionErrorProvider value)
		{
			obj.SetValue(CollectionErrorProviderProperty, value);
		}

		public static SubtreeErrorProvider GetSubtreeErrorProvider(DependencyObject obj)
		{
			return (SubtreeErrorProvider)obj.GetValue(SubtreeErrorProviderProperty);
		}

		public static void SetSubtreeErrorProvider(DependencyObject obj, SubtreeErrorProvider value)
		{
			obj.SetValue(SubtreeErrorProviderProperty, value);
		}

		#endregion

		#region Methods

		/// <summary>
		/// Насильно запускает процессинг очереди валидаций
		/// </summary>
		public static void ProcessPendingValidations()
		{
			ValidationQueue.ForceProcessQueue();
		}

		protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
		{
			base.OnRenderSizeChanged(sizeInfo);
			Dispatcher.BeginInvoke(DispatcherPriority.ContextIdle, (Action)InvalidateAdornerLayer);
		}

		private void InvalidateAdornerLayer()
		{
			var layer = AdornerLayer;
			if (layer != null)
				layer.InvalidateArrange();
		}

		/// <summary>
		/// [Re-]Initialize Error Provider with inner content and data context
		/// </summary>
		private void Initialize()
		{
			_view = this.FindParent<IView>();
			if (_view != null)
			{
				_view.Closed -= OnViewClosed;
				_view.Closed += OnViewClosed;
			}
			Clear();
			FindBindingsRecursively(this, true, ProcessBoundElement);

			foreach (var e in _forcedValidationElements)
				FindBindingsRecursively(e, true, ProcessBoundElement);

			InitializeDataObjects();

			ProcessValidation();
		}

		/// <summary>
		/// Initialize Data Objects Info from DataContext as Root and Control Bindings
		/// </summary>
		private void InitializeDataObjects()
		{
			_dataObjects[string.Empty] = new DataObjectInfo(this, string.Empty, DataContext);

			foreach (var e in _controlledElements)
			{
				string bindingPath;
				if (e.Value.CollectionErrorProvider != null)
				{
					var cep = e.Value.CollectionErrorProvider;
					bindingPath = cep.GetValidatingCollectionPath();
					if (!bindingPath.IsNullOrWhiteSpace())
					{
						AddBindingPath(bindingPath, false);
						if (!_dataObjects.ContainsKey(bindingPath))
						{
							_dataObjects[bindingPath] = new CollectionObjectInfo(this, bindingPath, cep);
						}
					}
					continue;
				}
				foreach (var bp in e.Value.BindingPathes)
				{
					AddBindingPath(bp, GetValidateObject(e.Value.Element));
				}
				if (e.Value.SubtreeErrorProvider != null)
				{
					bindingPath = e.Value.SubtreeErrorProvider.GetRootPath();
					DataObjectInfo doi;
					if (!_dataObjects.TryGetValue(bindingPath, out doi))
					{
						doi = new DataObjectInfo(this, bindingPath);
						_dataObjects[bindingPath] = doi;
					}
					doi.AddSubtree(e.Value.SubtreeErrorProvider);
				}
			}
		    if (ValidateBindings != null)
		    {
		        foreach (var bindingInfo in ValidateBindings)
		        {
		            if (bindingInfo.Binding != null && bindingInfo.Binding.Path != null)
		            {
		                var path = bindingInfo.Binding.Path.Path;
		                if (path != null)
		                {
                            AddBindingPath(path, true);
		                }
		            }
		}
		    }
		}

		private void AddBindingPath(string bindingPath, bool addFullPath)
		{
		    if (addFullPath && !_dataObjects.ContainsKey(bindingPath))
		    {
                _dataObjects[bindingPath] = new DataObjectInfo(this, bindingPath);
            }
			for (var n = bindingPath.LastIndexOf('.'); n >= 0; n = bindingPath.LastIndexOf('.'))
			{
				bindingPath = bindingPath.Substring(0, n);
				if (_dataObjects.ContainsKey(bindingPath))
					break;
				_dataObjects[bindingPath] = new DataObjectInfo(this, bindingPath);
			}
		}

		/// <summary>
		/// Get default data-bound property for element type.
		/// </summary>
		/// <param name="elementType">
		/// The framework element type.
		/// </param>
		/// <returns>
		/// Bound dependency property.
		/// </returns>
		private static IEnumerable<DependencyProperty> GetBoundProperties(Type elementType)
		{
			var checkedType = elementType;

			DependencyProperty[] dependencyProperty = null;

			// ReSharper disable once ConditionIsAlwaysTrueOrFalse
			while (checkedType != null && dependencyProperty == null
				&& !ElementTypesWithDependencyProperties.TryGetValue(checkedType, out dependencyProperty))
			{
				checkedType = checkedType.BaseType;
			}

			return dependencyProperty;
		}

		/// <summary>
		/// Вызывается для каждого найденного вложенного элемента, связанного с данными
		/// </summary>
		/// <param name="element"></param>
		private void ProcessBoundElement(FrameworkElement element)
		{
			CollectionErrorProvider cep;
			if ((cep = GetCollectionErrorProvider(element)) != null)
			{
				ProcessCollectionBoundElement(cep, element);
				return;
			}

			SubtreeErrorProvider sep;
			if ((sep = GetSubtreeErrorProvider(element)) != null)
			{
				ProcessSubtreeBoundElement(sep, element);
				return;
			}

			var boundProperty = ValidateProperty;
			var binding = BindingOperations.GetBinding(element, boundProperty);

			ContentPresenter presenter;
	

			IList<Binding> additiveBindings = null;
			if (binding == null || binding.Path == null || string.IsNullOrWhiteSpace(binding.Path.Path))
			{
				var properties = GetBoundProperties(element.GetType());
				// var defaultBoundProperty = GetBoundProperty(element.GetType());
				if (properties != null)
				{
					foreach (var dependencyProperty1 in properties)
					{
						var dependencyProperty = dependencyProperty1;
						boundProperty = DependencyPropertyCache.Instance[element.GetType()].FirstOrDefault(p => p == dependencyProperty);
						if (boundProperty != null)
						{
							if (binding == null)
							{
								binding = BindingOperations.GetBinding(element, boundProperty);
							}
							else
							{
								if (additiveBindings == null)
									additiveBindings = new List<Binding>();
								var b = BindingOperations.GetBinding(element, boundProperty);
								if (b != null)
								{
									additiveBindings.Add(b);
								}
							}
						}
					}
				}
			}

			if (binding != null && binding.Path != null && !string.IsNullOrWhiteSpace(binding.Path.Path))
			{
				var info = new ControlInfo(element, this, binding.Path.Path);
				_controlledElements[element] = info;
				if (additiveBindings != null)
				{
					foreach (var additiveBinding in additiveBindings)
					{
						if (additiveBinding != null && additiveBinding.Path != null && !string.IsNullOrWhiteSpace(additiveBinding.Path.Path))
						{
							info.AddBindingPath(additiveBinding.Path.Path);
						}
					}
				}
			}
		}

		private void ProcessSubtreeBoundElement(SubtreeErrorProvider sep, FrameworkElement element)
		{
			if (sep == null)
				return;
			sep.Initialize(this);
			_controlledElements[element] = new ControlInfo(element, this, sep.GetRootPath())
			{
				SubtreeErrorProvider = sep
			};
		}

		private void ProcessCollectionBoundElement(CollectionErrorProvider cep, FrameworkElement element)
		{
			if (cep == null)
				return;
			cep.Initialize(this, element);
			_controlledElements[element] = new ControlInfo(element, this, cep.GetValidatingCollectionPath())
			{
				CollectionErrorProvider = cep
			};
		}

		private void UnInitialize()
		{
			Clear();
			Loaded -= OnErrorProviderLoaded;
			DataContextChanged -= OnErrorProviderDataContextChanged;
		}

		private void Clear()
		{
			foreach (var pair in _dataObjects)
			{
				pair.Value.UnBind();
			}
			foreach (var pair in _controlledElements)
			{
				pair.Value.UnBind();
			}
			_dataObjects.Clear();
			_controlledElements.Clear();
		}

		/// <summary>
		/// Постановка в очередь вызова валидации
		/// </summary>
		internal void QueueValidate()
		{
			ValidationQueue.Enqueue(this);
		}

		/// <summary>
		/// Валидация всех объектов, контролируемых ErrorProvider-ом
		/// </summary>
		private void ProcessValidation()
		{
			_justHasErrors = false;
			_justErrorMessage = null;
			// Проверка изменений в промежуточных объектах дерева биндингов
			foreach (var pair in _dataObjects)
			{
				if (!pair.Key.IsNullOrEmpty())
					pair.Value.ReEvaluate();
			}
			
			List<ObjectValidationResult> validationErrors = null;
			var requiredProperties = new HashSet<string>();
			// Выполнение всех валидаций
			foreach (var pair in _dataObjects)
			{
				var results = pair.Value.ExecuteValidators(requiredProperties, null);
				if (results != null)
				{
				    foreach (var result in results)
				    {
				        if (validationErrors == null)
				            validationErrors = new List<ObjectValidationResult>();
				        validationErrors.Add(result);
				}
			}
			}
			// ErrorMessage = validationErrors.Count > 0 ? validationErrors[0].ErrorMessage : null;

			ShowRequiredMarks(requiredProperties);
			ShowValidationMarks(validationErrors);

			HasError = _justHasErrors;
			ErrorMessage = _justErrorMessage;
		}

		private void ShowRequiredMarks(HashSet<string> requiredProperties)
		{
			foreach (var p in _controlledElements)
			{
			    var any = false;
			    foreach (var s in p.Value.BindingPathes)
			    {
                    if (requiredProperties.Contains(s))
			            any = true;
			        break;
			    }
			    p.Value.IsDataRequired = any;
				p.Value.RefreshRequiredMark();
			}
		}

		/// <summary>
		/// Показ отметок об ошибках валидации
		/// </summary>
		/// <param name="validationErrors"></param>
		private void ShowValidationMarks([CanBeNull]List<ObjectValidationResult> validationErrors)
		{
			foreach (var p in _controlledElements)
			{
				p.Value.ClearTemporaryErrorInfo();
			}

		    if (validationErrors != null)
		    {
		        HashSet<ControlInfo> collectionInfos = null;

			foreach (var e in validationErrors)
			{
				if (e.CollectionItem != null)
				{
		                foreach (var controlInfo in _controlledElements.Values)
					{
		                    if (!controlInfo.CanShowErrorForProperty(e.CollectionPath))
		                        continue;
						if (controlInfo.CollectionErrorProvider != null)
						{
		                        if (collectionInfos == null)
		                            collectionInfos = new HashSet<ControlInfo>();
							collectionInfos.Add(controlInfo);
							controlInfo.ProvideCollectionError(e);
						}
						else
						{
							AddErrorToControl(controlInfo, e);
						}
					}
					continue;
				}
		            foreach (var controlInfo in _controlledElements.Values)
				{
		                if (controlInfo.CanShowErrorForProperty(e.PropertyName))
		                    AddErrorToControl(controlInfo, e);
				}
			}
		        if (collectionInfos != null)
		        {
			foreach (var controlInfo in collectionInfos)
			{
				controlInfo.BuildCollectionErrors();
				if (controlInfo.ErrorType == ValidationErrorType.Error)
				{
					_justHasErrors = true;
					if (_justErrorMessage.IsNullOrWhiteSpace())
						_justErrorMessage = controlInfo.ErrorMessage;
				}
			}
		        }
		    }
			foreach (var p in _controlledElements)
				p.Value.RefreshValidationMark();

		var groupe =	_controlledElements
				.Select(pair => new { ScopeElement = GetErrorScopeControl(pair.Key), ControlErorInfo = pair.Value })
				.Where(e => e.ScopeElement != null)
				.GroupBy(e => e.ScopeElement);


        foreach (var g in groupe)
        {
            var firstError = g.Select(e => e.ControlErorInfo)
                .FirstOrDefault(e => e.ErrorType == ValidationErrorType.Error);
            if (firstError != null)
            {
                SetErrorInfo(g.Key, new ErrorMarkInfo(ValidationErrorType.Error, firstError.ErrorMessages));
            }
            else
            {
                firstError = g.Select(e => e.ControlErorInfo)
                    .FirstOrDefault(e => e.ErrorType == ValidationErrorType.Warning);
                if (firstError != null)
                    SetErrorInfo(g.Key, new ErrorMarkInfo(ValidationErrorType.Warning, firstError.ErrorMessages));
                else
                    SetErrorInfo(g.Key, null);
            }
        }



        if (IsValidateNonVisual)
			{
				if (!_justHasErrors && _justErrorMessage.IsNullOrWhiteSpace() && validationErrors != null)
				{
					var arr = validationErrors.Where(e => e.ErrorType == ValidationErrorType.Error).ToArray();
					if (arr.Length > 0)
					{
						_justErrorMessage = string.Join(Environment.NewLine, arr.Select(e => e.ErrorMessage));
						_justHasErrors = true;
					}
				}
			}
		}

		private void AddErrorToControl(ControlInfo controlInfo, ObjectValidationResult e)
		{
			if (controlInfo == null)
				return;
			if (e.ErrorType == ValidationErrorType.Error)
			{
				_justHasErrors = true;
				if (_justErrorMessage.IsNullOrWhiteSpace())
					_justErrorMessage = e.ErrorMessage;
			}
			if (controlInfo.ErrorType > e.ErrorType)
				return;
			if (controlInfo.ErrorType != e.ErrorType)
			{
				controlInfo.ErrorType = e.ErrorType;
				controlInfo.ClearErrorMessage();
			}
			if (!string.IsNullOrEmpty(controlInfo.ErrorMessage))
			{
				if (controlInfo.ErrorMessages.Count > 6)
					return;
			}
			controlInfo.AddErrorMessage(e.ErrorMessage);
		}

		/// <summary>
		/// Recursively goes through the control tree, looking for bindings on the current data context.
		/// </summary>
		/// <param name="element">
		/// The root element to start searching at.
		/// </param>
		/// <param name="isValidateState">
		/// The is validate state.
		/// </param>
		/// <param name="callbackDelegate">
		/// A delegate called when a binding if found.
		/// </param>
		private void FindBindingsRecursively(DependencyObject element, bool isValidateState, Action<FrameworkElement> callbackDelegate)
		{
			// Вложенный ErrorProvider сам разберется со своим контентом
			if (!(element is ErrorProvider) || ReferenceEquals(element, this))
			{
				SetErrorProvider(element, this);
				var elementValidateState = GetIsValidate(element);
				isValidateState = elementValidateState ?? isValidateState;

				if (isValidateState && element is FrameworkElement
					&& (((FrameworkElement)element).DataContext == DataContext || _forcedValidationElements.Contains(element)))
				{
					callbackDelegate((FrameworkElement)element);
				}

				// Now, recurse through any child elements
				if ((element is FrameworkElement) || (element is FrameworkContentElement))
				{
					var subtree = GetSubtree(element) as FrameworkElement;
					if (subtree != null)
					{
						FindBindingsRecursively(subtree, true, callbackDelegate);
					}

					foreach (var childElement in LogicalTreeHelper.GetChildren(element).OfType<DependencyObject>())
					{
						FindBindingsRecursively(childElement, isValidateState, callbackDelegate);
					}
				}
			}
		}

		/// <summary>
		/// Register <see cref="FrameworkElement"/> types with dependency property.
		/// </summary>
		/// <param name="dependencyProperties">
		/// The list of types with dependency property.
		/// </param>
		public static void Register(Dictionary<Type, DependencyProperty> dependencyProperties)
		{
			foreach (var pair in dependencyProperties)
                ElementTypesWithDependencyProperties[pair.Key] = new[] { pair.Value };
		}

		public static void Register(Type type, params DependencyProperty[] dependencyProperties)
		{
			ElementTypesWithDependencyProperties[type] = dependencyProperties;
		}

		#endregion

		#region Event Handlers

		private void OnErrorProviderLoaded(object sender, RoutedEventArgs e)
		{
			if (DesignerHelper.IsInDesignModeStatic)
				return;
			Initialize();
		}

		private void OnViewClosed(object sender, EventArgs e)
		{
			_isViewUnloaded = true;
			UnInitialize();
			_view = null;
		}

		private void OnErrorProviderDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
		{
			Dispatcher.BeginInvoke((Action)Initialize, DispatcherPriority.Render);
		}

		#endregion

		public static readonly DependencyProperty ErrorScopeControlProperty =
			DependencyProperty.RegisterAttached("ErrorScopeControl", typeof(DependencyObject), typeof(ErrorProvider), new FrameworkPropertyMetadata(default(DependencyObject)) { Inherits = true });

		public static readonly DependencyPropertyKey ErrorInfoPropertyKey =
			DependencyProperty.RegisterAttachedReadOnly("ErrorInfo", typeof(ErrorMarkInfo), typeof(ErrorProvider), new PropertyMetadata(default(ErrorMarkInfo)));

		public static readonly DependencyProperty ErrorInfoProperty = ErrorInfoPropertyKey.DependencyProperty;

		public static readonly DependencyProperty ErrorControlProperty = DependencyProperty.RegisterAttached("ErrorControl", typeof(ErrorControl), typeof(ErrorProvider), new PropertyMetadata(default(ErrorControl)));

		public static ErrorControl GetErrorControl(UIElement element)
		{
			return (ErrorControl)element.GetValue(ErrorControlProperty);
		}

		public static void SetErrorControl(UIElement element, ErrorControl value)
		{
			element.SetValue(ErrorControlProperty, value);
		}

		public static void SetErrorScopeControl(DependencyObject element, DependencyObject value)
		{
			element.SetValue(ErrorScopeControlProperty, value);
		}

		public static DependencyObject GetErrorScopeControl(DependencyObject element)
		{
			return (DependencyObject)element.GetValue(ErrorScopeControlProperty);
		}

		private static void SetErrorInfo(DependencyObject element, ErrorMarkInfo value)
		{
			element.SetValue(ErrorInfoPropertyKey, value);
		}

		public static ErrorMarkInfo GetErrorInfo(DependencyObject element)
		{
			return (ErrorMarkInfo)element.GetValue(ErrorInfoProperty);
		}
	}

    public class ErrorProviderBinding
    {
        public Binding Binding { get; set; }
	}
}
