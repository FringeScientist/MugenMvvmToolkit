﻿using System;
using System.Threading;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Models;

namespace MugenMvvmToolkit.Test.TestModels
{
    public abstract class DisposableObject : IDisposableObject
    {
        #region Fields

        private const int DisposingState = 1;
        private const int DisposedState = 2;
        private int _disposed;

        #endregion

        #region Implementation of IDisposableObject

        public void Dispose()
        {
            if (Interlocked.CompareExchange(ref _disposed, DisposingState, 0) != 0)
                return;
            try
            {
                GC.SuppressFinalize(this);
                OnDispose(true);
                RaiseDisposed();
                Disposed = null;
            }
            finally
            {
                _disposed = DisposedState;
            }
        }

        public bool IsDisposed => _disposed == DisposedState;

        public event EventHandler<IDisposableObject, EventArgs> Disposed;

        #endregion

        #region Methods

        protected void EnsureNotDisposed()
        {
            this.NotBeDisposed();
        }

        protected virtual void OnDispose(bool disposing)
        {
        }

        private void RaiseDisposed()
        {
            EventHandler<IDisposableObject, EventArgs> handler = Disposed;
            if (handler != null)
                handler(this, EventArgs.Empty);
        }

        #endregion

        #region Destructor

        ~DisposableObject()
        {
            OnDispose(false);
        }

        #endregion
    }
}
