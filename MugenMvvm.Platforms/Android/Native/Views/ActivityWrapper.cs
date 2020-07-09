﻿using Android.Runtime;
using MugenMvvm.Collections;
using MugenMvvm.Interfaces.Internal;

namespace MugenMvvm.Android.Native.Views
{
    public partial class ActivityWrapper : IWeakReference, IValueHolder<LightDictionary<string, object?>>
    {
        #region Fields

        private bool _isAlive = true;

        #endregion

        #region Properties

        bool IWeakItem.IsAlive => _isAlive;

        object? IWeakReference.Target
        {
            get
            {
                if (_isAlive)
                    return this;
                return null;
            }
        }

        [Preserve(Conditional = true)]
        LightDictionary<string, object?>? IValueHolder<LightDictionary<string, object?>>.Value { get; set; }

        #endregion

        #region Implementation of interfaces

        void IWeakReference.Release()
        {
            _isAlive = false;
            ((IValueHolder<LightDictionary<string, object?>>)this).Value?.Clear();
        }

        #endregion
    }
}
