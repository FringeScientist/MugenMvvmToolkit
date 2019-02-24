﻿using System.Collections.Generic;
using MugenMvvm.Collections;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Infrastructure.Internal
{
    public abstract class HasListenersBase<T> : IHasListeners<T> where T : class, IListener
    {
        #region Fields

        private LightArrayList<T>? _listeners;

        #endregion

        #region Properties

        protected LightArrayList<T>? Listeners => _listeners;

        protected bool HasListeners => _listeners != null;

        #endregion

        #region Implementation of interfaces

        public void AddListener(T listener)
        {
            Should.NotBeNull(listener, nameof(listener));
            if (_listeners == null)
                MugenExtensions.LazyInitialize(ref _listeners, new LightArrayList<T>(2));
            _listeners!.AddWithLock(listener);
        }

        public void RemoveListener(T listener)
        {
            Should.NotBeNull(listener, nameof(listener));
            _listeners?.RemoveWithLock(listener);
        }

        public IReadOnlyList<T> GetListeners()
        {
            if (_listeners == null)
                return Default.EmptyArray<T>();
            return Listeners.ToArrayWithLock();
        }

        #endregion

        #region Methods

        protected T?[] GetListenersInternal()
        {
            if (_listeners == null)
                return Default.EmptyArray<T?>();
            return _listeners.GetRawItems(out _);
        }

        #endregion
    }
}