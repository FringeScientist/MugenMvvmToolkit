﻿using System;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Metadata.Components;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.UnitTest.Metadata
{
    public class TestMetadataContextProviderListener : IMetadataContextProviderListener, IHasPriority
    {
        #region Properties

        public Action<IMetadataContextProvider, IReadOnlyMetadataContext, object?>? OnReadOnlyContextCreated { get; set; }

        public Action<IMetadataContextProvider, IMetadataContext, object?>? OnContextCreated { get; set; }

        public int Priority { get; set; }

        #endregion

        #region Implementation of interfaces

        void IMetadataContextProviderListener.OnReadOnlyContextCreated(IMetadataContextProvider metadataContextProvider, IReadOnlyMetadataContext metadataContext, object? target)
        {
            OnReadOnlyContextCreated?.Invoke(metadataContextProvider, metadataContext, target);
        }

        void IMetadataContextProviderListener.OnContextCreated(IMetadataContextProvider metadataContextProvider, IMetadataContext metadataContext, object? target)
        {
            OnContextCreated?.Invoke(metadataContextProvider, metadataContext, target);
        }

        #endregion
    }
}