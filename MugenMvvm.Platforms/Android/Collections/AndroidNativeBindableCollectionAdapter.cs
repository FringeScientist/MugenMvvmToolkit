﻿using System;
using System.Collections.Generic;
using MugenMvvm.Android.Native.Interfaces;
using MugenMvvm.Collections;

namespace MugenMvvm.Android.Collections
{
    public sealed class AndroidNativeBindableCollectionAdapter : BindableCollectionAdapterBase<object?>
    {
        #region Fields

        private bool _isAlive;

        #endregion

        #region Constructors

        public AndroidNativeBindableCollectionAdapter()
        {
            Observers = new List<IItemsSourceObserver>();
            _isAlive = true;
        }

        #endregion

        #region Properties

        protected override bool IsAlive => _isAlive;

        public List<IItemsSourceObserver> Observers { get; }

        #endregion

        #region Methods

        protected override void OnAdded(object? item, int index, bool batch)
        {
            base.OnAdded(item, index, batch);
            for (var i = 0; i < Observers.Count; i++)
                GetObserver(i)?.OnItemInserted(index);
        }

        protected override void OnMoved(object? item, int oldIndex, int newIndex, bool batch)
        {
            base.OnMoved(item, oldIndex, newIndex, batch);
            for (var i = 0; i < Observers.Count; i++)
                GetObserver(i)?.OnItemMoved(oldIndex, newIndex);
        }

        protected override void OnRemoved(object? item, int index, bool batch)
        {
            base.OnRemoved(item, index, batch);
            for (var i = 0; i < Observers.Count; i++)
                GetObserver(i)?.OnItemRemoved(index);
        }

        protected override void OnReplaced(object? oldItem, object? newItem, int index, bool batch)
        {
            base.OnReplaced(oldItem, newItem, index, batch);
            for (var i = 0; i < Observers.Count; i++)
                GetObserver(i)?.OnItemChanged(index);
        }

        protected override void OnReset(IEnumerable<object?> items, bool batch)
        {
            base.OnReset(items, batch);
            for (var i = 0; i < Observers.Count; i++)
                GetObserver(i)?.OnReset();
        }

        protected override void OnCleared(bool batch)
        {
            base.OnCleared(batch);
            for (var i = 0; i < Observers.Count; i++)
                GetObserver(i)?.OnReset();
        }

        private IItemsSourceObserver? GetObserver(int index)
        {
            var observer = Observers[index];
            if (observer.Handle == IntPtr.Zero)
            {
                _isAlive = false;
                return null;
            }

            return observer;
        }

        #endregion
    }
}