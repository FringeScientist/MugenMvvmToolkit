﻿using System.Threading.Tasks;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.ViewModels;

namespace MugenMvvm.Interfaces.Views.Infrastructure
{
    public interface IViewManagerInitializer : IHasMetadata<IReadOnlyMetadataContext>, IHasId<string>
    {
        Task<IViewManagerResult> InitializeAsync(IViewModelBase viewModel, object view, IReadOnlyMetadataContext metadata);
    }
}