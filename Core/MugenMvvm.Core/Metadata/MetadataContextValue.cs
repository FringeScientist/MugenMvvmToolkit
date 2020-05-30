﻿using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Internal;

namespace MugenMvvm.Metadata
{
    [StructLayout(LayoutKind.Auto)]
    public readonly struct MetadataContextValue
    {
        #region Fields

        public readonly IMetadataContextKey ContextKey;
        public readonly object? Value;

        internal static readonly Func<KeyValuePair<IMetadataContextKey, object?>, MetadataContextValue> CreateDelegate = Create;

        #endregion

        #region Constructors

        private MetadataContextValue(KeyValuePair<IMetadataContextKey, object?> pair)
            : this(pair.Key, pair.Value)
        {
        }

        private MetadataContextValue(IMetadataContextKey contextKey, object? value)
        {
            Should.NotBeNull(contextKey, nameof(contextKey));
            ContextKey = contextKey;
            Value = value;
        }

        #endregion

        #region Properties

        public bool IsEmpty => ContextKey == null;

        #endregion

        #region Methods

        public IReadOnlyMetadataContext ToContext()
        {
            return new SingleValueMetadataContext(this);
        }

        public static MetadataContextValue Create(KeyValuePair<IMetadataContextKey, object?> pair)
        {
            return new MetadataContextValue(pair);
        }

        public static MetadataContextValue Create<TGet, TSet>(IMetadataContextKey<TGet, TSet> contextKey, TSet value)
        {
            Should.NotBeNull(contextKey, nameof(contextKey));
            return new MetadataContextValue(contextKey, contextKey.SetValue(Default.Metadata, null, value));
        }

        #endregion
    }
}