﻿using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.ViewModels;

namespace MugenMvvm.Interfaces.Views
{
    public interface IViewModelProviderViewManagerComponent : IComponent<IViewManager>
    {
        IViewModelBase? TryGetViewModelForView(IViewInitializer initializer, object view, IReadOnlyMetadataContext metadata);
    }
}