﻿using System;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.Wrapping
{
    public interface IWrapperManagerFactory
    {
        bool CanWrap(IWrapperManager wrapperManager, Type type, Type wrapperType, IReadOnlyMetadataContext metadata);

        object? TryWrap(IWrapperManager wrapperManager, object item, Type wrapperType, IReadOnlyMetadataContext metadata);
    }
}