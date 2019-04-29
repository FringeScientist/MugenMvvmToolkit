﻿using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.ViewModels;

namespace MugenMvvm.Interfaces.Views.Infrastructure
{
    public interface IParentViewManager : IViewManager
    {
        void OnViewModelCreated(IViewModelBase viewModel, object view, IReadOnlyMetadataContext metadata);

        void OnViewCreated(object view, IViewModelBase viewModel, IReadOnlyMetadataContext metadata);

        void OnViewInitialized(IViewInfo viewInfo, IViewModelBase viewModel, IReadOnlyMetadataContext metadata);

        void OnViewCleared(IViewInfo viewInfo, IViewModelBase viewModel, IReadOnlyMetadataContext metadata);
    }
}