﻿using MugenMvvm.Attributes;
using MugenMvvm.Components;
using MugenMvvm.Extensions.Components;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Internal;
using MugenMvvm.Interfaces.Internal.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Internal
{
    public sealed class WeakReferenceManager : ComponentOwnerBase<IWeakReferenceManager>, IWeakReferenceManager
    {
        #region Fields

        private readonly ComponentTracker _componentTracker;
        private IWeakReferenceProviderComponent[]? _components;

        #endregion

        #region Constructors

        [Preserve(Conditional = true)]
        public WeakReferenceManager(IComponentCollectionManager? componentCollectionManager = null) : base(componentCollectionManager)
        {
            _componentTracker = new ComponentTracker();
            _componentTracker.AddListener<IWeakReferenceProviderComponent, WeakReferenceManager>((components, state, _) => state._components = components, this);
        }

        #endregion

        #region Implementation of interfaces

        public IWeakReference? TryGetWeakReference(object? item, IReadOnlyMetadataContext? metadata = null)
        {
            if (item == null)
                return Default.WeakReference;
            if (_components == null)
                _componentTracker.Attach(this, metadata);
            return _components!.TryGetWeakReference(this, item, metadata);
        }

        #endregion
    }
}