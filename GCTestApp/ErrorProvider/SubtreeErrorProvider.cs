using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Data;
using FluentValidation;

namespace GCTestApp.ErrorProvider
{
	public class SubtreeErrorProvider : FrameworkElement
	{
		public static readonly DependencyProperty RootProperty =
			DependencyProperty.Register("Root", typeof(object), typeof(SubtreeErrorProvider), new PropertyMetadata(null, OnRootChanged));

		private object _boundRoot;

		private static void OnRootChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			var sep = d as SubtreeErrorProvider;
			if (sep != null)
				sep.RebindSubtree();
		}

		public IEnumerable<ErrorProvider.ObjectValidationResult> ValidateSubtree(IServiceProvider serviceProvider)
		{
			if (Root == null)
				return null;
			List<ErrorProvider.ObjectValidationResult> errorList = null;
			//var ndei = Root as INotifyDataErrorInfo;
			//if (ndei != null)
			//{
			//	if (serviceProvider != null)
			//		ndei.Validate(serviceProvider);
			//	else
			//		ndei.Validate();
			//	var errors = ndei.GetErrors(null);
			//	if (errors != null)
			//	{
			//		ValidationUtilites.AddValidationResults(null, ref errorList, errors);
			//	}
			//}
			//else
			{
				var hasValidator = Root as IHasFluentValidator;
				IValidator validator;
				if (hasValidator != null && (validator = hasValidator.GetValidator()) != null)
				{
					var errors = ValidationUtilites.ValidateViaValidator(Root, validator, null, serviceProvider);
					if (errors != null)
					{
						ValidationUtilites.AddValidationResults(null, ref errorList, errors);
					}
				}
			}
			return errorList;
		}

		private void RebindSubtree()
		{
			if (ReferenceEquals(Root, _boundRoot))
				return;
			UnBind();
			if (Root != null)
			{
				var obj = Root;
				var npc = obj as INotifyPropertyChanged;
				if (npc != null)
					npc.PropertyChanged += OnRootPropertyChanged;
				_boundRoot = obj;
			}
		}

		private void UnBind()
		{
			if (_boundRoot != null)
			{
				var obj = _boundRoot;
				_boundRoot = null;
				var npc = obj as INotifyPropertyChanged;
				if (npc != null)
					npc.PropertyChanged -= OnRootPropertyChanged;
			}
		}

		public void UnInitialize()
		{
			UnBind();
		}

		private void OnRootPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			//if (e.IsSpecialProperty())
			//	return;
			if (ErrorProvider != null)
				ErrorProvider.QueueValidate();
		}

		public object Root
		{
			get { return GetValue(RootProperty); }
			set { SetValue(RootProperty, value); }
		}

		public ErrorProvider ErrorProvider { get; set; }

		public void Initialize(ErrorProvider errorProvider)
		{
			ErrorProvider = errorProvider;
			if (errorProvider != null)
				DataContext = ErrorProvider.DataContext;
		}

		public string GetRootPath()
		{
			var b = BindingOperations.GetBinding(this, RootProperty);
			if (b == null || b.Path == null || b.Path.Path == null)
				return string.Empty;
			return b.Path.Path;
		}
	}
}