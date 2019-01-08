﻿using System;
using JetBrains.Annotations;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.BusyIndicator;
using MugenMvvm.Interfaces.Messaging;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Interfaces.ViewModels.Infrastructure
{
    public interface IViewModelDispatcher : IHasListeners<IViewModelDispatcherListener>
    {
        [Pure]
        IBusyIndicatorProvider GetBusyIndicatorProvider(IViewModel viewModel, IReadOnlyMetadataContext metadata);

        [Pure]
        IMessenger GetMessenger(IViewModel viewModel, IReadOnlyMetadataContext metadata);

        [Pure]
        IObservableMetadataContext GetMetadataContext(IViewModel viewModel, IReadOnlyMetadataContext metadata);


        void OnLifecycleChanged(IViewModel viewModel, ViewModelLifecycleState lifecycleState, IReadOnlyMetadataContext metadata);

        [Pure]
        IViewModel? GetViewModelById(Guid id, IReadOnlyMetadataContext metadata);
    }
}