using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;

namespace GCTestApp.ErrorProvider
{
	internal class CollectionWatcher
	{
		internal class BoundItemInfo
		{
			private CollectionWatcher _collectionWatcher;
			private INotifyPropertyChanged _npc;

			public BoundItemInfo(CollectionWatcher collectionWatcher, INotifyPropertyChanged npc)
			{
				_collectionWatcher = collectionWatcher;
				_npc = npc;
				_npc.PropertyChanged += OnNpcPropertyChanged;
			}

			private void OnNpcPropertyChanged(object sender, PropertyChangedEventArgs e)
			{
                // NOTE: Entity.ValidationErrors постоянно сообщает о своём изменении.
                if (e.PropertyName == "ValidationErrors" || e.PropertyName == "HasValidationErrors")
                    return;

				if (_collectionWatcher != null)
					_collectionWatcher.OnItemPropertyChanged(sender, e);
			}

			public void Unbind()
			{
				_npc.PropertyChanged -= OnNpcPropertyChanged;
				_npc = null;
				_collectionWatcher = null;
			}
		}

		private void OnItemPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
		{
			if (_ep != null)
				_ep.OnCollectionPropertyChanged(sender, propertyChangedEventArgs);
		}

		private INotifyPropertyChanged _notifyPropertyChanged;
		private INotifyCollectionChanged _notifyCollectionChanged;
		private IEnumerable _enumerable;
		private readonly CollectionErrorProvider _ep;
		private readonly IDictionary<object, BoundItemInfo> _boundItems = new Dictionary<object, BoundItemInfo>(ObjectReferenceEqualityComparer.Default);

		public CollectionWatcher(IEnumerable validatingCollection, CollectionErrorProvider ep)
		{
			_notifyPropertyChanged = validatingCollection as INotifyPropertyChanged;
			if (_notifyPropertyChanged != null)
				_notifyPropertyChanged.PropertyChanged += OnNotifyPropertyChanged;
			_notifyCollectionChanged = validatingCollection as INotifyCollectionChanged;
			if (_notifyCollectionChanged != null)
				_notifyCollectionChanged.CollectionChanged += OnNotifyCollectionChanged;
			_enumerable = validatingCollection;
			_ep = ep;

			AddBoundItems(_enumerable.OfType<object>());
		}

		private void AddBoundItems(IEnumerable items)
		{
			if(items==null) return;
			foreach (var obj in items)
			{
				var npc = obj as INotifyPropertyChanged;
				if (npc != null)
				{
					_boundItems[npc] = new BoundItemInfo(this, npc);
				}
			}
		}

		private void OnNotifyCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			if (e.Action == NotifyCollectionChangedAction.Reset)
			{
				ClearBoundItems();
				AddBoundItems(_enumerable.OfType<object>());
			}
			if (RemoveBoundItems(e.OldItems))
			{
				AddBoundItems(e.NewItems);
			}
			else
			{
				ClearBoundItems();
				AddBoundItems(_enumerable.OfType<object>());
			}
		}

		private bool RemoveBoundItems(IEnumerable oldItems)
		{
			if (oldItems == null)
				return true;
			foreach (var item in oldItems.OfType<INotifyPropertyChanged>())
			{
				BoundItemInfo bii;
				if (!_boundItems.TryGetValue(item, out bii))
					return false;
				bii.Unbind();
				_boundItems.Remove(item);
			}
			return true;
		}

		private void OnNotifyPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (_ep != null)
				_ep.OnCollectionPropertyChanged(sender, e);
		}

		public void Unbind()
		{
			if (_notifyPropertyChanged != null)
			{
				_notifyPropertyChanged.PropertyChanged -= OnNotifyPropertyChanged;
				_notifyPropertyChanged = null;
			}
			if (_notifyCollectionChanged != null)
			{
				_notifyCollectionChanged.CollectionChanged -= OnNotifyCollectionChanged;
				_notifyCollectionChanged = null;
			}
			ClearBoundItems();
			_enumerable = null;
		}

		private void ClearBoundItems()
		{
			foreach (var boundItemInfo in _boundItems)
			{
				boundItemInfo.Value.Unbind();
			}
			_boundItems.Clear();
		}
	}
}
