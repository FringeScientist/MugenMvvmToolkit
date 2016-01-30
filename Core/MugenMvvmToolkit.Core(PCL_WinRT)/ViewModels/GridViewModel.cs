﻿#region Copyright

// ****************************************************************************
// <copyright file="GridViewModel.cs">
// Copyright (c) 2012-2016 Vyacheslav Volkov
// </copyright>
// ****************************************************************************
// <author>Vyacheslav Volkov</author>
// <email>vvs0205@outlook.com</email>
// <project>MugenMvvmToolkit</project>
// <web>https://github.com/MugenMvvmToolkit/MugenMvvmToolkit</web>
// <license>
// See license.txt in this solution or http://opensource.org/licenses/MS-PL
// </license>
// ****************************************************************************

#endregion

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using MugenMvvmToolkit.Annotations;
using MugenMvvmToolkit.Collections;
using MugenMvvmToolkit.Infrastructure;
using MugenMvvmToolkit.Interfaces.Collections;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Interfaces.ViewModels;
using MugenMvvmToolkit.Models;
using MugenMvvmToolkit.Models.EventArg;

namespace MugenMvvmToolkit.ViewModels
{
    [BaseViewModel(Priority = 7)]
    public class GridViewModel<T> : ViewModelBase, IGridViewModel<T> where T : class
    {
        #region Fields

        private FilterDelegate<T> _filter;
        private INotifiableCollection<T> _itemsSource;
        private IList<T> _originalData;
        private FilterableNotifiableCollection<T> _filterableItemsSource;
        private T _selectedItem;

        private EventHandler<IGridViewModel, SelectedItemChangedEventArgs> _selectedItemChangedNonGeneric;
        private EventHandler<IGridViewModel, ItemsSourceChangedEventArgs> _itemsSourceChangedNonGeneric;

        #endregion

        #region Constructors

        public GridViewModel()
        {
            SetOriginalItemsSource(new SynchronizedNotifiableCollection<T>());
            UpdateSelectedStateOnChange = false;
        }

        #endregion

        #region Propreties

        protected FilterableNotifiableCollection<T> FilterableItemsSource => _filterableItemsSource;

        public bool UpdateSelectedStateOnChange { get; set; }

        #endregion

        #region Implementation of IGridViewModel

        Type IGridViewModel.ModelType => typeof(T);

        IList IGridViewModel.OriginalItemsSource => (IList)OriginalItemsSource;

        INotifiableCollection IGridViewModel.ItemsSource => (INotifiableCollection)ItemsSource;

        object IGridViewModel.SelectedItem
        {
            get { return SelectedItem; }
            set { SelectedItem = (T)value; }
        }

        FilterDelegate<object> IGridViewModel.Filter
        {
            set { Filter = value; }
        }

        public virtual FilterDelegate<T> Filter
        {
            get { return _filter; }
            set
            {
                if (Equals(value, _filter)) return;
                _filter = value;
                UpdateFilter();
                OnPropertyChanged();
            }
        }

        void IGridViewModel.UpdateItemsSource(IEnumerable value)
        {
            UpdateItemsSource((IEnumerable<T>)value);
        }

        public virtual IList<T> OriginalItemsSource => FilterableItemsSource.SourceCollection;

        public virtual INotifiableCollection<T> ItemsSource => _itemsSource;

        public virtual T SelectedItem
        {
            get { return _selectedItem; }
            set
            {
                if (EqualityComparer<T>.Default.Equals(_selectedItem, value))
                    return;
                T oldValue = _selectedItem;
                _selectedItem = OnSelectedItemChanging(value);
                if (EqualityComparer<T>.Default.Equals(_selectedItem, oldValue))
                    return;

                if (_selectedItem != null)
                {
                    FilterDelegate<T> filter = Filter;
                    if (filter != null && !filter(_selectedItem))
                        _selectedItem = null;
                }

                if (UpdateSelectedStateOnChange)
                {
                    var selectable = oldValue as ISelectable;
                    if (selectable != null)
                        selectable.IsSelected = false;

                    selectable = _selectedItem as ISelectable;
                    if (selectable != null)
                        selectable.IsSelected = true;
                }

                OnSelectedItemChanged(oldValue, _selectedItem);
                RaiseSelectedItemChanged(oldValue, _selectedItem);
                OnPropertyChanged(Empty.SelectedItemChangedArgs);
            }
        }

        public void UpdateItemsSource(IEnumerable<T> value)
        {
            UpdateItemsSourceInternal(value);
        }

        public void SetOriginalItemsSource<TItemsSource>(TItemsSource originalItemsSource)
            where TItemsSource : IList<T>, INotifyCollectionChanged, IList
        {
            EnsureNotDisposed();
            Should.NotBeNull(originalItemsSource, nameof(originalItemsSource));
            INotifyCollectionChanging collectionChanging;
            if (_originalData != null)
            {
                collectionChanging = _originalData as INotifyCollectionChanging;
                if (collectionChanging != null)
                    collectionChanging.CollectionChanging -= RaiseCollectionChanging;
                ((INotifyCollectionChanged)_originalData).CollectionChanged -= RaiseCollectionChanged;
                if (_originalData.Count != 0)
                    originalItemsSource.AddRange(_originalData);
            }
            _filterableItemsSource = new FilterableNotifiableCollection<T>(originalItemsSource);
            collectionChanging = originalItemsSource as INotifyCollectionChanging;
            if (collectionChanging != null)
                collectionChanging.CollectionChanging += RaiseCollectionChanging;
            originalItemsSource.CollectionChanged += RaiseCollectionChanged;

            _originalData = originalItemsSource;
            var list = ServiceProvider.TryDecorate(this, FilterableItemsSource);
            Should.BeOfType<INotifiableCollection<T>>(list, "DecoratedItemsSource");
            _itemsSource = (INotifiableCollection<T>)list;
            UpdateFilter();
            OnPropertyChanged(nameof(ItemsSource));
            OnPropertyChanged(nameof(OriginalItemsSource));
            RaiseItemsSourceChanged(_itemsSource);
        }

        public void UpdateFilter()
        {
            UpdateFilterInternal();
        }

        event EventHandler<IGridViewModel, SelectedItemChangedEventArgs> IGridViewModel.SelectedItemChanged
        {
            add { _selectedItemChangedNonGeneric += value; }
            remove { _selectedItemChangedNonGeneric -= value; }
        }

        event EventHandler<IGridViewModel, ItemsSourceChangedEventArgs> IGridViewModel.ItemsSourceChanged
        {
            add { _itemsSourceChangedNonGeneric += value; }
            remove { _itemsSourceChangedNonGeneric -= value; }
        }

        public virtual event EventHandler<IGridViewModel, SelectedItemChangedEventArgs<T>> SelectedItemChanged;

        public virtual event EventHandler<IGridViewModel, ItemsSourceChangedEventArgs<T>> ItemsSourceChanged;

        public virtual event NotifyCollectionChangingEventHandler CollectionChanging;

        public virtual event NotifyCollectionChangedEventHandler CollectionChanged;

        #endregion

        #region Methods

        protected virtual void UpdateItemsSourceInternal(IEnumerable<T> value)
        {
            value = OnItemsSourceChanging(value);
            SelectedItem = null;

            if (value == null)
                _originalData.Clear();
            else
            {
                using (ItemsSource.SuspendNotifications())
                {
                    _originalData.Clear();
                    _itemsSource.AddRange(value);
                }
            }
            UpdateFilter();
            OnItemsSourceChanged(value);
            RaiseItemsSourceChanged(value);
            OnPropertyChanged(nameof(ItemsSource));
            OnPropertyChanged(nameof(OriginalItemsSource));
        }

        protected virtual void UpdateFilterInternal()
        {
            FilterDelegate<T> filter = Filter;
            if (filter == FilterableItemsSource.Filter)
                FilterableItemsSource.UpdateFilter();
            else
                FilterableItemsSource.Filter = filter;
            if (SelectedItem != null && filter != null && !filter(SelectedItem))
                SelectedItem = null;
        }

        protected virtual T OnSelectedItemChanging(T newValue)
        {
            return newValue;
        }

        protected virtual void OnSelectedItemChanged(T oldValue, T newValue)
        {
        }

        protected virtual IEnumerable<T> OnItemsSourceChanging(IEnumerable<T> data)
        {
            return data;
        }

        protected virtual void OnItemsSourceChanged(IEnumerable<T> data)
        {
        }

        protected virtual void RaiseSelectedItemChanged(T oldValue, T newValue)
        {
            if (SelectedItemChanged == null && _selectedItemChangedNonGeneric == null)
                return;
            var args = new SelectedItemChangedEventArgs<T>(oldValue, newValue);
            ThreadManager.Invoke(Settings.EventExecutionMode, this, args, (model, eventArgs) =>
            {
                model.SelectedItemChanged?.Invoke(model, eventArgs);
                model._selectedItemChangedNonGeneric?.Invoke(model, eventArgs);
            });
        }

        protected virtual void RaiseItemsSourceChanged(IEnumerable<T> data)
        {
            if (ItemsSourceChanged == null && _itemsSourceChangedNonGeneric == null)
                return;
            var args = new ItemsSourceChangedEventArgs<T>(data);
            ThreadManager.Invoke(Settings.EventExecutionMode, this, args, (model, eventArgs) =>
            {
                model.ItemsSourceChanged?.Invoke(model, eventArgs);
                model._itemsSourceChangedNonGeneric?.Invoke(model, eventArgs);
            });
        }

        protected virtual void OnCollectionChanging(object sender, NotifyCollectionChangingEventArgs args)
        {
        }

        protected virtual void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs args)
        {
        }

        private void RaiseCollectionChanging(object sender, NotifyCollectionChangingEventArgs e)
        {
            OnCollectionChanging(sender, e);
            CollectionChanging?.Invoke(this, e);
        }

        private void RaiseCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            OnCollectionChanged(sender, e);
            CollectionChanged?.Invoke(this, e);
        }

        #endregion

        #region Overrides of ViewModelBase

        public override IDisposable SuspendNotifications()
        {
            var baseToken = base.SuspendNotifications();
            var collectionToken = ItemsSource.SuspendNotifications();
            return new ActionToken(() =>
            {
                baseToken.Dispose();
                collectionToken.Dispose();
            });
        }

        internal override void OnDisposeInternal(bool disposing)
        {
            if (disposing)
            {
                _itemsSourceChangedNonGeneric = null;
                _selectedItemChangedNonGeneric = null;
                SelectedItemChanged = null;
                ItemsSourceChanged = null;
                CollectionChanging = null;
                CollectionChanging = null;
            }
            base.OnDisposeInternal(disposing);
        }

        #endregion
    }
}
