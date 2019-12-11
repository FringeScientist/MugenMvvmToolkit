﻿using System.Collections.Generic;
using MugenMvvm.Binding.Extensions.Components;
using MugenMvvm.Binding.Interfaces.Observers;
using MugenMvvm.Binding.Interfaces.Observers.Components;
using MugenMvvm.Collections.Internal;
using MugenMvvm.Components;
using MugenMvvm.Constants;
using MugenMvvm.Delegates;
using MugenMvvm.Interfaces.Internal;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Binding.Observers.Components
{
    public sealed class CacheMemberPathProviderComponent : DecoratorComponentBase<IObserverProvider, IMemberPathProviderComponent>, IHasPriority, IMemberPathProviderComponent, IHasCache
    {
        #region Fields

        private readonly StringOrdinalLightDictionary<IMemberPath> _cache;
        private readonly FuncIn<string, IReadOnlyMetadataContext?, IMemberPath?> _getMemberPathStringDelegate;

        #endregion

        #region Constructors

        public CacheMemberPathProviderComponent()
        {
            _cache = new StringOrdinalLightDictionary<IMemberPath>(59);
            _getMemberPathStringDelegate = TryGetMemberPath;
        }

        #endregion

        #region Properties

        public int Priority { get; set; } = ComponentPriority.Cache;

        #endregion

        #region Implementation of interfaces

        public void Invalidate(object? state = null, IReadOnlyMetadataContext? metadata = null)
        {
            _cache.Clear();
        }

        public IMemberPath? TryGetMemberPath<TPath>(in TPath path, IReadOnlyMetadataContext? metadata)
        {
            if (_getMemberPathStringDelegate is FuncIn<TPath, IReadOnlyMetadataContext?, IMemberPath?> provider)
                return provider.Invoke(path, metadata);
            return null;
        }

        #endregion

        #region Methods

        protected override void OnAttachedInternal(IObserverProvider owner, IReadOnlyMetadataContext? metadata)
        {
            base.OnAttachedInternal(owner, metadata);
            Invalidate();
        }

        protected override void OnDetachedInternal(IObserverProvider owner, IReadOnlyMetadataContext? metadata)
        {
            base.OnDetachedInternal(owner, metadata);
            Invalidate();
        }

        protected override void DecorateInternal(IList<IMemberPathProviderComponent> components, IReadOnlyMetadataContext? metadata)
        {
            base.DecorateInternal(components, metadata);
            Invalidate();
        }

        private IMemberPath? TryGetMemberPath(in string path, IReadOnlyMetadataContext? metadata)
        {
            if (!_cache.TryGetValue(path, out var value))
            {
                value = Components.TryGetMemberPath(path, metadata)!;
                if (value == null)
                    return null;
                _cache[path] = value;
            }

            return value;
        }

        #endregion
    }
}