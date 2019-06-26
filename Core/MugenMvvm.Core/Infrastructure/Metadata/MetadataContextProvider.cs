﻿using System.Collections.Generic;
using MugenMvvm.Attributes;
using MugenMvvm.Infrastructure.Components;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Infrastructure.Metadata
{
    public sealed class MetadataContextProvider : ComponentOwnerBase<IMetadataContextProvider>, IMetadataContextProvider
    {
        #region Constructors

        [Preserve(Conditional = true)]
        public MetadataContextProvider(IComponentCollectionProvider componentCollectionProvider) : base(componentCollectionProvider)
        {
        }

        #endregion

        #region Implementation of interfaces

        public IReadOnlyMetadataContext GetReadOnlyMetadataContext(object? target, IEnumerable<MetadataContextValue>? values)
        {
            var components = Components.GetItems();
            IReadOnlyMetadataContext? result = null;
            for (var i = 0; i < components.Length; i++)
            {
                result = (components[i] as IMetadataContextProviderComponent)?.TryGetReadOnlyMetadataContext(target, values);
                if (result != null)
                    break;
            }

            if (result == null)
                ExceptionManager.ThrowObjectNotInitialized(this, typeof(IMetadataContextProviderComponent).Name);

            for (var i = 0; i < components.Length; i++)
                (components[i] as IMetadataContextProviderListener)?.OnReadOnlyContextCreated(this, result, target);
            return result;
        }

        public IMetadataContext GetMetadataContext(object? target, IEnumerable<MetadataContextValue>? values)
        {
            var components = Components.GetItems();
            IMetadataContext? result = null;
            for (var i = 0; i < components.Length; i++)
            {
                result = (components[i] as IMetadataContextProviderComponent)?.TryGetMetadataContext(target, values);
                if (result != null)
                    break;
            }

            if (result == null)
                ExceptionManager.ThrowObjectNotInitialized(this, typeof(IMetadataContextProviderComponent).Name);

            for (var i = 0; i < components.Length; i++)
                (components[i] as IMetadataContextProviderListener)?.OnContextCreated(this, result, target);
            return result;
        }

        #endregion
    }
}