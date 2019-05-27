using System.Windows;

namespace GCTestApp.ErrorProvider
{
    /// <summary>
    /// 
    /// </summary>
	public abstract class CollectionControlErrorProvider : DependencyObject
	{
		protected FrameworkElement CollectionControl
		{
			get { return CollectionErrorProvider == null ? null : CollectionErrorProvider.CollectionControl; }
		}

		protected CollectionErrorProvider CollectionErrorProvider { get; set; }

		public virtual void Unbind()
		{

		}

		public void Bind(CollectionErrorProvider collectionErrorProvider)
		{
			CollectionErrorProvider = collectionErrorProvider;
			BindToControl();
		}

		protected virtual void BindToControl()
		{
		}

		public ErrorMarkInfo GetItemError(object collectionItem)
		{
			if (CollectionErrorProvider == null)
				return null;
			return CollectionErrorProvider.GetItemError(collectionItem);
		}

		public abstract void RefreshValidationMark();
	}
}