﻿using System.Collections;
using System.Collections.Generic;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Internal;

namespace MugenMvvm.Metadata
{
    public sealed class SingleValueMetadataContext : IReadOnlyMetadataContext
    {
        #region Fields

        private readonly MetadataContextValue _value;

        #endregion

        #region Constructors

        public SingleValueMetadataContext(MetadataContextValue value)
        {
            Should.NotBeNull(value.ContextKey, nameof(value));
            _value = value;
        }

        #endregion

        #region Properties

        public int Count => 1;

        #endregion

        #region Implementation of interfaces

        public IEnumerator<MetadataContextValue> GetEnumerator()
        {
            return Default.SingleValueEnumerator(_value);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public bool TryGetRaw(IMetadataContextKey contextKey, out object? value)
        {
            if (Contains(contextKey))
            {
                value = _value.Value;
                return true;
            }

            value = null;
            return false;
        }

        public bool Contains(IMetadataContextKey contextKey)
        {
            return _value.ContextKey.Equals(contextKey);
        }

        #endregion
    }
}