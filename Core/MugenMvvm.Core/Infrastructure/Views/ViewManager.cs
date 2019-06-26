﻿using System.Collections.Generic;
using MugenMvvm.Infrastructure.Components;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.ViewModels;
using MugenMvvm.Interfaces.Views;

namespace MugenMvvm.Infrastructure.Views
{
    public class ViewManager : ComponentOwnerBase<IViewManager>, IViewManager
    {
        #region Constructors

        public ViewManager(IComponentCollectionProvider componentCollectionProvider)
            : base(componentCollectionProvider)
        {
        }

        #endregion

        #region Implementation of interfaces

        public IReadOnlyList<IViewInfo> GetViews(IViewModelBase viewModel, IReadOnlyMetadataContext metadata)
        {
            Should.NotBeNull(viewModel, nameof(viewModel));
            Should.NotBeNull(metadata, nameof(metadata));
            return GetViewsInternal(viewModel, metadata);
        }

        public IReadOnlyList<IViewInitializer> GetInitializersByView(object view, IReadOnlyMetadataContext metadata)
        {
            Should.NotBeNull(view, nameof(view));
            Should.NotBeNull(metadata, nameof(metadata));
            return GetInitializersByViewInternal(view, metadata);
        }

        public IReadOnlyList<IViewInitializer> GetInitializersByViewModel(IViewModelBase viewModel, IReadOnlyMetadataContext metadata)
        {
            Should.NotBeNull(viewModel, nameof(viewModel));
            Should.NotBeNull(metadata, nameof(metadata));
            return GetInitializersByViewModelInternal(viewModel, metadata);
        }

        #endregion

        #region Methods

        protected virtual IReadOnlyList<IViewInfo> GetViewsInternal(IViewModelBase viewModel, IReadOnlyMetadataContext metadata)
        {
            var components = Components.GetItems();
            if (components.Length == 0)
                return Default.EmptyArray<IViewInfo>();

            var result = new List<IViewInfo>();
            for (var i = 0; i < components.Length; i++)
            {
                if (components[i] is IViewInfoProviderComponent component)
                    result.AddRange(component.GetViews(viewModel, metadata));
            }

            return result;
        }

        protected virtual IReadOnlyList<IViewInitializer> GetInitializersByViewInternal(object view, IReadOnlyMetadataContext metadata)
        {
            var components = Components.GetItems();
            if (components.Length == 0)
                return Default.EmptyArray<IViewInitializer>();

            var result = new List<IViewInitializer>();
            for (var i = 0; i < components.Length; i++)
            {
                if (components[i] is IViewInitializerProviderComponent component)
                    result.AddRange(component.GetInitializersByView(view, metadata));
            }

            return result;
        }

        protected virtual IReadOnlyList<IViewInitializer> GetInitializersByViewModelInternal(IViewModelBase viewModel, IReadOnlyMetadataContext metadata)
        {
            var components = Components.GetItems();
            if (components.Length == 0)
                return Default.EmptyArray<IViewInitializer>();

            var result = new List<IViewInitializer>();
            for (var i = 0; i < components.Length; i++)
            {
                if (components[i] is IViewInitializerProviderComponent component)
                    result.AddRange(component.GetInitializersByViewModel(viewModel, metadata));
            }

            return result;
        }

        #endregion
    }
}