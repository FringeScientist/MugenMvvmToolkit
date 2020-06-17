﻿using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using MugenMvvm.Binding.Enums;
using MugenMvvm.Binding.Extensions.Components;
using MugenMvvm.Binding.Interfaces.Core;
using MugenMvvm.Binding.Interfaces.Core.Components;
using MugenMvvm.Components;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Internal;

namespace MugenMvvm.Binding.Core
{
    public sealed class BindingManager : ComponentOwnerBase<IBindingManager>, IBindingManager
    {
        #region Fields

        private readonly ComponentTracker _componentTracker;
        private IBindingExpressionBuilderComponent[]? _expressionBuilderComponents;
        private IBindingHolderComponent[]? _holderComponents;
        private IBindingLifecycleDispatcherComponent[]? _stateDispatcherComponents;

        #endregion

        #region Constructors

        public BindingManager(IComponentCollectionProvider? componentCollectionProvider = null)
            : base(componentCollectionProvider)
        {
            _componentTracker = new ComponentTracker();
            _componentTracker.AddListener<IBindingExpressionBuilderComponent, BindingManager>((components, state, _) => state._expressionBuilderComponents = components, this);
            _componentTracker.AddListener<IBindingHolderComponent, BindingManager>((components, state, _) => state._holderComponents = components, this);
            _componentTracker.AddListener<IBindingLifecycleDispatcherComponent, BindingManager>((components, state, _) => state._stateDispatcherComponents = components, this);
        }

        #endregion

        #region Implementation of interfaces

        public ItemOrList<IBindingExpression, IReadOnlyList<IBindingExpression>> TryBuildBindingExpression<TExpression>([DisallowNull]in TExpression expression, IReadOnlyMetadataContext? metadata = null)
        {
            if (_expressionBuilderComponents == null)
                _componentTracker.Attach(this, metadata);
            return _expressionBuilderComponents!.TryBuildBindingExpression(expression, metadata);
        }

        public ItemOrList<IBinding, IReadOnlyList<IBinding>> GetBindings(object target, string? path = null, IReadOnlyMetadataContext? metadata = null)
        {
            if (_holderComponents == null)
                _componentTracker.Attach(this, metadata);
            return _holderComponents!.TryGetBindings(target, path, metadata);
        }

        public void OnLifecycleChanged<TState>(IBinding binding, BindingLifecycleState lifecycleState, in TState state, IReadOnlyMetadataContext? metadata = null)
        {
            if (_stateDispatcherComponents == null)
                _componentTracker.Attach(this, metadata);
            _stateDispatcherComponents!.OnLifecycleChanged(binding, lifecycleState, state, metadata);
        }

        #endregion
    }
}