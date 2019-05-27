using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using FluentValidation;
using JetBrains.Annotations;

namespace GCTestApp.ErrorProvider
{
    partial class ErrorProvider
    {
	    /// <summary>
        /// 
        /// </summary>
        private class DataObjectInfo
        {

			private static class AmbiguousPropertyCache
			{
				private static readonly IDictionary<KeyValuePair<Type,string>, PropertyInfo> _cache
					= new Dictionary<KeyValuePair<Type, string>, PropertyInfo>(new CacheEqualityComparer());
				private static readonly object SyncLock = new object();

				private sealed class CacheEqualityComparer : IEqualityComparer<KeyValuePair<Type, string>>
				{
					public bool Equals(KeyValuePair<Type, string> x, KeyValuePair<Type, string> y)
					{
						return x.Key == y.Key && x.Value == y.Value;
					}

					public int GetHashCode(KeyValuePair<Type, string> obj)
					{
						return (obj.Key).GetHashCode() ^ (obj.Value).GetHashCode();
					}
				}

				public static PropertyInfo ResolveAmbiguity(Type type, string propertyName)
				{
					var t = type;
					PropertyInfo property = null;
					while (t != null)
					{
						property = t.GetProperties(BindingFlags.Public | BindingFlags.DeclaredOnly | BindingFlags.Instance).FirstOrDefault(p => p.Name == propertyName);
						if (property != null)
							break;
						t = t.BaseType;
					}
					lock (SyncLock)
					{
						_cache[new KeyValuePair<Type, string>(type, propertyName)] = property;
					}
					return property;
				}

				public static bool TryGetProperty(Type type, string propertyName, out PropertyInfo propertyInfo)
				{
					lock (SyncLock)
					{
						return _cache.TryGetValue(new KeyValuePair<Type, string>(type, propertyName), out propertyInfo);
					}
				}
			}

            /// <summary>
            /// Value of this Data Object
            /// </summary>
            private object _value;

            /// <summary>
            /// List of excluded property names.
            /// </summary>
            private static readonly HashSet<string> ExcludedPropertyNames = new HashSet<string> {
                "HasError", "ErrorMessage", "IsReadOnly", "IsBusy", "HasBusyServices", "BusyStatusMessage", "BusyElapsed",
                "Model.Error", "HasValidationErrors", "ValidationErrors", "Model.IsReadOnly"
            };

		    private List<SubtreeErrorProvider> _subtreeErrorProviders;

		    public DataObjectInfo(ErrorProvider errorProvider, string bindingPath, object value = null)
            {
                ErrorProvider = errorProvider;
                BindingPath = bindingPath;
                Value = value;
            }

            protected string BindingPath
            {
                get; }

            protected object Value
            {
                get { return _value; }
            	private set
                {
                    var oldValue = _value;
                    if (oldValue == value)
                        return;
                    if (oldValue != null)
                        UnBind();
                    _value = value;
                    Bind();
                }
            }

            private INotifyPropertyChanged NotifyPropertyChanged
            {
                get;
                set;
            }

            /// <summary>
            /// Parent ErrorProvider
            /// </summary>
            private ErrorProvider ErrorProvider
            {
                get; }

            /// <summary>
            /// Bind To Object
            /// </summary>
            private void Bind()
            {
                NotifyPropertyChanged = Value as INotifyPropertyChanged;
                if (NotifyPropertyChanged != null)
                    NotifyPropertyChanged.PropertyChanged += OnObjectPropertyChanged;
            }

            /// <summary>
            /// UnBind from Value
            /// </summary>
            public void UnBind()
            {
                if (NotifyPropertyChanged != null)
                    NotifyPropertyChanged.PropertyChanged -= OnObjectPropertyChanged;
                NotifyPropertyChanged = null;
            }

            /// <summary>
            /// Event Handler For Property Changed
            /// </summary>
            /// <param name="sender"></param>
            /// <param name="e"></param>
            private void OnObjectPropertyChanged(object sender, PropertyChangedEventArgs e)
            {
                if (ExcludedPropertyNames.Contains(e.PropertyName))
                    return;
                if (!BindingPath.IsNullOrEmpty() && ExcludedPropertyNames.Contains(BindingPath + "." + e.PropertyName))
                    return;
                var n = e.PropertyName.LastIndexOf('.');
                if (n >= 0 && ExcludedPropertyNames.Contains(e.PropertyName.Substring(n + 1)))
                    return;
                ErrorProvider.QueueValidate();
            }

            /// <summary>
            /// Перевычисление значения объекта
            /// </summary>
            public void ReEvaluate()
            {
                object x = ErrorProvider.DataContext;
                var propPath = BindingPath.Split('.');
                foreach (var propertyName in propPath)
                {
                    if (x == null)
                        break;
                	PropertyInfo pi;
                	var t = x.GetType();
                	if (!AmbiguousPropertyCache.TryGetProperty(t, propertyName, out pi))
						pi = GetSafePropertyInfo(t, propertyName);
                    x = (pi == null) ? null : pi.GetValue(x, null);
                }
                Value = x;
            }

        	[DebuggerHidden]
			private static PropertyInfo GetSafePropertyInfo(Type type, string propertyName)
        	{
        		PropertyInfo pi;
        		try
        		{
        			pi = type.GetProperty(propertyName);
        		}
        		catch (AmbiguousMatchException)
        		{
        			pi = AmbiguousPropertyCache.ResolveAmbiguity(type, propertyName);
        		}
        		return pi;
        	}

        	/// <summary>
            /// Вызов валидации данной сущности и возврат набора ошибок валидации
            /// </summary>
            /// <param name="requiredProperties"></param>
            /// <param name="serviceProvider"></param>
            /// <returns></returns>
            public virtual IEnumerable<ObjectValidationResult> ExecuteValidators(HashSet<string> requiredProperties, IServiceProvider serviceProvider)
        	{
        		var retVal = ValidateOneObject(Value, BindingPath, requiredProperties, serviceProvider);
        		if (_subtreeErrorProviders != null)
        		{
        			foreach (var subtreeErrorProvider in _subtreeErrorProviders)
        			{
        				ValidateSubtree(subtreeErrorProvider, ref retVal, BindingPath, serviceProvider);
        			}
        		}
        		return retVal;
        	}

		    private void ValidateSubtree(SubtreeErrorProvider subtreeErrorProvider, ref List<ObjectValidationResult> retVal, string bindingPath, IServiceProvider serviceProvider)
		    {
			    var subtreeResult = subtreeErrorProvider.ValidateSubtree(serviceProvider);
			    if (subtreeResult != null)
			    {
				    foreach (var v in subtreeResult)
				    {
					    v.PropertyName = bindingPath;
					    if (retVal == null)
					    {
						    retVal = new List<ObjectValidationResult>();
					    }
						retVal.Add(v);
					}
			    }
		    }

			protected List<ObjectValidationResult> ValidateOneObject(object val, string objectPath, 
				[CanBeNull]HashSet<string> requiredProperties, IServiceProvider serviceProvider,
				bool useStandardValidators = true,
				IValidator additiveValidator = null
				)
			{
				List<ObjectValidationResult> errorList = null;

				if (useStandardValidators)
				{
                    // Закоментил кастомный INotifyDataErrorInfo
                    //var ndei = val as INotifyDataErrorInfo;
                    //if (ndei != null)
                    //{
                    //	if (serviceProvider != null)
                    //		ndei.Validate(serviceProvider);
                    //	else
                    //		ndei.Validate();

                    //	var errors = ndei.GetErrors(null);
                    //	if (errors != null)
                    //	{
                    //		ValidationUtilites.AddValidationResults(objectPath, ref errorList, errors);
                    //	}
                    //	var required = ndei.GetRequiredProperties();
                    //	if (required != null && requiredProperties != null)
                    //	{
                    //		foreach (var propertyName in required)
                    //		{
                    //			requiredProperties.Add(ValidationUtilites.BuildPropertyPath(objectPath, propertyName));
                    //		}
                    //	}
                    //}

                    var hasValidator = val as IHasFluentValidator;
					IValidator validator;
					if (hasValidator != null && (validator = hasValidator.GetValidator()) != null)
					{
						ValidateOneObjectViaValidator(val, objectPath, requiredProperties, serviceProvider, validator, ref errorList);
					}

				}
                if (additiveValidator != null)
                {
                    ValidateOneObjectViaValidator(val, objectPath, requiredProperties, serviceProvider, additiveValidator, ref errorList);
                }
                return errorList;
			}

		    private static void ValidateOneObjectViaValidator(object val, string objectPath, HashSet<string> requiredProperties, IServiceProvider serviceProvider, IValidator validator, ref List<ObjectValidationResult> errorList)
		    {
			    var required = new HashSet<string>();
			    var errors = ValidationUtilites.ValidateViaValidator(val, validator, required, serviceProvider);
			    if (errors != null)
			    {
				    ValidationUtilites.AddValidationResults(objectPath, ref errorList, errors);
			    }
			    if (requiredProperties != null)
			    {
				    foreach (var propertyName in required)
				    {
					    requiredProperties.Add(ValidationUtilites.BuildPropertyPath(objectPath, propertyName));
				    }
			    }
		    }

		    public void AddSubtree(SubtreeErrorProvider subtreeErrorProvider)
		    {
			    if (_subtreeErrorProviders == null)
				    _subtreeErrorProviders = new List<SubtreeErrorProvider>();
				_subtreeErrorProviders.Add(subtreeErrorProvider);
		    }
        }

		private sealed class CollectionObjectInfo : DataObjectInfo
		{
			private readonly CollectionErrorProvider _collectionErrorProvider;

			public CollectionObjectInfo(ErrorProvider errorProvider, string bindingPath, CollectionErrorProvider collectionErrorProvider) 
				: base(errorProvider, bindingPath)
			{
				_collectionErrorProvider = collectionErrorProvider;
			}

			public override IEnumerable<ObjectValidationResult> ExecuteValidators(HashSet<string> requiredProperties, IServiceProvider serviceProvider)
			{
				var coll = Value as IEnumerable;
				var errorList = new List<ObjectValidationResult>();
				if (coll == null)
					return errorList;
				var req = new HashSet<string>();
				foreach (var v in coll)
				{
					var errors = ValidateOneObject(v, string.Empty, req, serviceProvider, _collectionErrorProvider.UseStandardValidators, _collectionErrorProvider.Validator);
					if (errors == null)
						continue;
					foreach (var error in errors)
					{
						error.CollectionItem = v;
						error.CollectionPath = BindingPath;
						errorList.Add(error);
					}
				}
				return errorList;
			}
		}
    }
}
