﻿using MugenMvvm.Constants;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.ViewModels;
using MugenMvvm.Interfaces.ViewModels.Components;
using MugenMvvm.Metadata;

namespace MugenMvvm.ViewModels.Components
{
    public sealed class ViewModelLifecycleTracker : IViewModelLifecycleDispatcherComponent, IHasPriority
    {
        #region Properties

        public int Priority { get; set; } = ViewModelComponentPriority.LifecycleTracker;

        #endregion

        #region Implementation of interfaces

        public void OnLifecycleChanged(IViewModelManager viewModelManager, IViewModelBase viewModel, ViewModelLifecycleState lifecycleState, object? state, IReadOnlyMetadataContext? metadata) =>
            viewModel.Metadata.Set(ViewModelMetadata.LifecycleState, lifecycleState, out _);

        #endregion
    }
}