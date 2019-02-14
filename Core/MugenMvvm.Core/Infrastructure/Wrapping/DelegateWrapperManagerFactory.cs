﻿using System;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Wrapping;

namespace MugenMvvm.Infrastructure.Wrapping
{
    public class DelegateWrapperManagerFactory : IWrapperManagerFactory
    {
        #region Fields

        public Func<IWrapperManager, Type, Type, IReadOnlyMetadataContext, bool> Condition;
        public Func<IWrapperManager, object, Type, IReadOnlyMetadataContext, object?> WrapperFactory;

        #endregion

        #region Constructors

        public DelegateWrapperManagerFactory(Func<IWrapperManager, Type, Type, IReadOnlyMetadataContext, bool> condition,
            Func<IWrapperManager, object, Type, IReadOnlyMetadataContext, object?> wrapperFactory)
        {
            Should.NotBeNull(condition, nameof(condition));
            Should.NotBeNull(wrapperFactory, nameof(wrapperFactory));
            Condition = condition;
            WrapperFactory = wrapperFactory;
        }

        #endregion

        #region Implementation of interfaces

        public bool CanWrap(IWrapperManager wrapperManager, Type type, Type wrapperType, IReadOnlyMetadataContext metadata)
        {
            return Condition(wrapperManager, type, wrapperType, metadata);
        }

        public object? TryWrap(IWrapperManager wrapperManager, object item, Type wrapperType, IReadOnlyMetadataContext metadata)
        {
            return WrapperFactory(wrapperManager, item, wrapperType, metadata);
        }

        #endregion
    }
}