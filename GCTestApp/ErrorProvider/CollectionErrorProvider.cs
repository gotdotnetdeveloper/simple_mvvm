using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Data;
using FluentValidation;

namespace GCTestApp.ErrorProvider
{
	public class CollectionErrorProvider : FrameworkElement
	{
		public ErrorProvider ErrorProvider { get; set; }

		internal CollectionControlErrorProvider ControlProvider {get; set;}

		public FrameworkElement CollectionControl { get; set; }

		private readonly IDictionary<object, ErrorMarkInfo> _objectErrors = new Dictionary<object, ErrorMarkInfo>();

		private static readonly Dictionary<Type, Type> ControlProviderRegistry = new Dictionary<Type, Type>();

		private CollectionWatcher _collectionWatcher;

		public static readonly DependencyProperty ValidatingCollectionProperty =
			DependencyProperty.Register("ValidatingCollection", typeof(IEnumerable), typeof(CollectionErrorProvider), new PropertyMetadata(null, OnValidatingCollectionChanged));
		public static readonly DependencyProperty WatchCollectionChangedProperty =
			DependencyProperty.Register("WatchCollectionChanged", typeof(bool), typeof(CollectionErrorProvider), new PropertyMetadata(false, OnWatchCollectionChangedChanged));
		public static readonly DependencyProperty UseStandardValidatorsProperty = DependencyProperty.Register(
			"UseStandardValidators", typeof(bool), typeof(CollectionErrorProvider), new PropertyMetadata(true, OnValidatorChanged));

		public static readonly DependencyProperty ValidatorProperty = DependencyProperty.Register(
			"Validator", typeof(IValidator), typeof(CollectionErrorProvider), new PropertyMetadata(default(IValidator), OnValidatorChanged));

		public IValidator Validator
		{
			get { return (IValidator)GetValue(ValidatorProperty); }
			set { SetValue(ValidatorProperty, value); }
		}

		private static void OnValidatorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			var cep = d as CollectionErrorProvider;
			if (cep != null && cep.ErrorProvider != null)
				cep.ErrorProvider.QueueValidate();
		}

		public bool UseStandardValidators
		{
			get { return (bool)GetValue(UseStandardValidatorsProperty); }
			set { SetValue(UseStandardValidatorsProperty, value); }
		}

		public IEnumerable ValidatingCollection
		{
			get { return (IEnumerable)GetValue(ValidatingCollectionProperty); }
			set { SetValue(ValidatingCollectionProperty, value); }
		}

		public bool WatchCollectionChanged
		{
			get { return (bool)GetValue(WatchCollectionChangedProperty); }
			set { SetValue(WatchCollectionChangedProperty, value); }
		}

		private static void OnValidatingCollectionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			var ctrl = d as CollectionErrorProvider;
			if (ctrl != null)
			{
				ctrl.UpdateCollectionBindings();
			}
		}

		private static void OnWatchCollectionChangedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			var ctrl = d as CollectionErrorProvider;
			if (ctrl != null)
			{
				ctrl.UpdateCollectionBindings();
			}
		}

		private void UpdateCollectionBindings()
		{
			UnbindCollectionWatcher();
			if (!WatchCollectionChanged || ValidatingCollection == null || ErrorProvider == null || CollectionControl == null)
				return;
			_collectionWatcher = new CollectionWatcher(ValidatingCollection, this);
		}

		private void UnbindCollectionWatcher()
		{
			if (_collectionWatcher != null)
			{
				_collectionWatcher.Unbind();
				_collectionWatcher = null;
			}
		}

		public void UnInitialize()
		{
			Initialize(null, null);
		}

		public void OnCollectionPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
		{
			if (ErrorProvider != null)
				ErrorProvider.QueueValidate();
		}

		public string GetValidatingCollectionPath()
		{
			var b = BindingOperations.GetBinding(this, ValidatingCollectionProperty);
			if (b == null || b.Path == null || b.Path.Path == null)
				return string.Empty;
			return b.Path.Path;
		}

		public void Initialize(ErrorProvider errorProvider, FrameworkElement collectionControl)
		{
			ErrorProvider = errorProvider;
			if (ErrorProvider == null)
			{
				BindingOperations.ClearBinding(this, DataContextProperty);
			}
			else
			{
				BindingOperations.SetBinding(this, DataContextProperty, new Binding("DataContext") {Source = ErrorProvider});
			}
			if (!ReferenceEquals(CollectionControl, collectionControl))
			{
				if (CollectionControl != null)
					UnbindFromControl();

				CollectionControl = collectionControl;
				if (CollectionControl != null)
					BindToControl();
			}
			UpdateCollectionBindings();
		}

		private void BindToControl()
		{
			var provider = CreateControlProvider(CollectionControl);
			ControlProvider = provider;
			if (provider != null)
			{
				provider.Bind(this);
			}
		}

		private void UnbindFromControl()
		{
			if (ControlProvider != null)
				ControlProvider.Unbind();
		}

		public static void RegisterControlProvider<TControl, TProvider>()
			where TControl : FrameworkElement
			where TProvider : CollectionControlErrorProvider, new()
		{
			ControlProviderRegistry[typeof(TControl)] = typeof(TProvider);
		}

		private static CollectionControlErrorProvider CreateControlProvider(FrameworkElement collectionControl)
		{
			if (collectionControl == null)
				return null;
			var t = collectionControl.GetType();
			while (t != null && t != typeof(object))
			{
				Type providerType;
				if (ControlProviderRegistry.TryGetValue(t, out providerType))
					return (CollectionControlErrorProvider)Activator.CreateInstance(providerType);
				t = t.BaseType;
			}
			return null;
		}

		public ErrorMarkInfo GetMostErrorMark()
		{
			var mark = (ErrorMarkInfo)null;
			foreach (var errorMarkInfo in _objectErrors)
			{
				if (mark == null || CompareErrorType(mark.ErrorType, errorMarkInfo.Value.ErrorType) < 0)
					mark = errorMarkInfo.Value;
			}
			return mark;
		}

		public void ClearErrors()
		{
			_objectErrors.Clear();
		}

		public void AddObjectError(ErrorProvider.ObjectValidationResult objectValidationResult)
		{
			ErrorMarkInfo r;
			if (!_objectErrors.TryGetValue(objectValidationResult.CollectionItem, out r))
			{
				_objectErrors[objectValidationResult.CollectionItem] = new ErrorMarkInfo(objectValidationResult.ErrorType, objectValidationResult.ErrorMessage);
			}
			else
			{
				if (r.ErrorType == objectValidationResult.ErrorType)
				{
					r.AddErrorMessage(objectValidationResult.ErrorMessage);
				}
				if (CompareErrorType(r.ErrorType, objectValidationResult.ErrorType) < 0)
				{
					r.ErrorType = objectValidationResult.ErrorType;
					r.ReplaceErrorMessage(objectValidationResult.ErrorMessage);
				}
			}
		}

		private static int CompareErrorType(ValidationErrorType e1, ValidationErrorType e2)
		{
			return ErrorTypeIndex(e1) - ErrorTypeIndex(e2);
		}

		private static int ErrorTypeIndex(ValidationErrorType e)
		{
			switch (e)
			{
				case ValidationErrorType.None:
					return 0;
				case ValidationErrorType.Warning:
					return 1;
				case ValidationErrorType.Server:
					return 2;
				case ValidationErrorType.Error:
					return 3;
			}
			return 0;
		}

		public ErrorMarkInfo GetItemError(object collectionItem)
		{
			ErrorMarkInfo error;
			if (_objectErrors.TryGetValue(collectionItem, out error))
				return error;
			return null;
		}

		public void RefreshValidationMark()
		{
			if (ControlProvider != null)
				ControlProvider.RefreshValidationMark();
		}

	}
}
