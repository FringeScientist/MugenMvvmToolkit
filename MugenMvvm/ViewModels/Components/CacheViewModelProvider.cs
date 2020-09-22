﻿using System;
using System.Collections.Generic;
using MugenMvvm.Constants;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Internal;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.ViewModels;
using MugenMvvm.Interfaces.ViewModels.Components;
using MugenMvvm.Metadata;

namespace MugenMvvm.ViewModels.Components
{
    public sealed class CacheViewModelProvider : IViewModelProviderComponent, IViewModelLifecycleDispatcherComponent, IHasPriority
    {
        #region Fields

        private readonly bool _isWeakCache;
        private readonly Dictionary<string, object> _viewModelsCache;

        #endregion

        #region Constructors

        public CacheViewModelProvider(bool isWeakCache = true)
        {
            _isWeakCache = isWeakCache;
            _viewModelsCache = new Dictionary<string, object>(StringComparer.Ordinal);
        }

        #endregion

        #region Properties

        public int Priority { get; set; } = ViewModelComponentPriority.Provider;

        public Func<IViewModelBase, object?, IReadOnlyMetadataContext?, bool>? ShouldCache { get; set; }

        #endregion

        #region Implementation of interfaces

        void IViewModelLifecycleDispatcherComponent.OnLifecycleChanged(IViewModelManager viewModelManager, IViewModelBase viewModel, ViewModelLifecycleState lifecycleState, object? state,
            IReadOnlyMetadataContext? metadata)
        {
            if (ShouldCache != null && !ShouldCache(viewModel, state, metadata))
                return;

            if (lifecycleState == ViewModelLifecycleState.Created)
            {
                var id = viewModel.GetId();
                Add(id, viewModel);
                viewModel.Metadata.Set(InternalMetadata.CreatedId, id, out _);
            }
            else if (lifecycleState == ViewModelLifecycleState.Restored)
            {
                var id = viewModel.GetId();
                var createdId = viewModel.Metadata.Get(InternalMetadata.CreatedId);
                if (createdId != id)
                {
                    Remove(createdId);
                    Add(id, viewModel);
                }
            }
            else if (lifecycleState == ViewModelLifecycleState.Disposed)
                Remove(viewModel.GetId());
        }

        public IViewModelBase? TryGetViewModel(IViewModelManager viewModelManager, object request, IReadOnlyMetadataContext? metadata)
        {
            if (!(request is string id))
                return null;

            object? value;
            lock (_viewModelsCache)
            {
                if (!_viewModelsCache.TryGetValue(id, out value))
                    return null;
            }

            if (!_isWeakCache)
                return (IViewModelBase) value;

            var vm = (IViewModelBase?) ((IWeakReference) value).Target;
            if (vm == null)
                Remove(id);
            return vm;
        }

        #endregion

        #region Methods

        private void Add(string? id, IViewModelBase viewModel)
        {
            if (id == null)
                return;
            lock (_viewModelsCache)
            {
                _viewModelsCache[id] = _isWeakCache ? (object) viewModel.ToWeakReference() : viewModel;
            }
        }

        private void Remove(string? id)
        {
            if (id == null)
                return;
            lock (_viewModelsCache)
            {
                _viewModelsCache.Remove(id);
            }
        }

        #endregion
    }
}