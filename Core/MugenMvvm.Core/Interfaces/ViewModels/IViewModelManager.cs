﻿using System;
using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.App;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.ViewModels
{
    //todo cleanup manager, clear commands, initialize manager, provider manager
    public interface IViewModelManager : IComponentOwner<IViewModelManager>, IComponent<IMugenApplication>
    {
        void OnLifecycleChanged<TState>(IViewModelBase viewModel, ViewModelLifecycleState lifecycleState, [AllowNull] in TState state, IReadOnlyMetadataContext? metadata = null);

        [Pure]
        object GetService(IViewModelBase viewModel, Type service, IReadOnlyMetadataContext? metadata = null);

        [Pure]
        IViewModelBase? TryGetViewModel<TRequest>([DisallowNull]in TRequest request, IReadOnlyMetadataContext? metadata = null);
    }
}