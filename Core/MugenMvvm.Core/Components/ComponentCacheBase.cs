﻿using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Internal;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Components
{
    public abstract class ComponentCacheBase<T, TComponent> : ComponentDecoratorBase<T, TComponent>, IComponentCollectionChangedListener, IHasCache
        where TComponent : class
        where T : class, IComponentOwner<T>
    {
        #region Implementation of interfaces

        void IComponentCollectionChangedListener.OnAdded(IComponentCollection collection, object component, IReadOnlyMetadataContext? metadata)
        {
            OnComponentAdded(collection, component, metadata);
        }

        void IComponentCollectionChangedListener.OnRemoved(IComponentCollection collection, object component, IReadOnlyMetadataContext? metadata)
        {
            OnComponentRemoved(collection, component, metadata);
        }

        public abstract void Invalidate<TState>(in TState state, IReadOnlyMetadataContext? metadata);

        #endregion

        #region Methods

        protected override void OnAttachedInternal(T owner, IReadOnlyMetadataContext? metadata)
        {
            base.OnAttachedInternal(owner, metadata);
            Invalidate<object?>(null, metadata);
        }

        protected override void OnDetachedInternal(T owner, IReadOnlyMetadataContext? metadata)
        {
            base.OnDetachedInternal(owner, metadata);
            Invalidate<object?>(null, metadata);
        }

        protected virtual void OnComponentAdded(IComponentCollection collection, object component, IReadOnlyMetadataContext? metadata)
        {
            if (component is TComponent)
                Invalidate<object?>(null, metadata);
        }

        protected virtual void OnComponentRemoved(IComponentCollection collection, object component, IReadOnlyMetadataContext? metadata)
        {
            if (component is TComponent)
                Invalidate<object?>(null, metadata);
        }

        #endregion
    }
}